using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Mnemo.Common;
using Mnemo.Contracts.Dtos.Memorization;
using Mnemo.Data;
using Mnemo.Data.Entities;

namespace Mnemo.Services
{
    public class VocabularyMemorizationService
    {
        private AppDbContext _context;
        private AccountService _accountService;
        private VocabularyManagementService _vocabularyService;

        private static Random _random = new Random();


        public VocabularyMemorizationService(AppDbContext context, AccountService accountService, VocabularyManagementService vocabularyService)
        {
            _context = context;
            _accountService = accountService;
            _vocabularyService = vocabularyService;
        }


        public async Task<RequestResult<RepetitionSession>> GetRepetitionSessionStatusAsync(int userId)
        {
            var session = await GetRepetitionSessionAsync(userId);
            if (session == null) return RequestResult<RepetitionSession>.Failure("SESSION_NOT_FOUND");
            if (session.IsFinished) return RequestResult<RepetitionSession>.Failure("SESSION_WAS_FINISHED");
            else return RequestResult<RepetitionSession>.Failure("SESSION_IN_PROCESS");
        }

        public async Task<RepetitionSession?> GetRepetitionSessionAsync(int userId)
        {
            return await _context.RepetitionSessions
                .Include(i => i.Tasks)
                .FirstOrDefaultAsync(e => e.User.Id == userId);
        }

        public async Task<List<RepetitionTask>> GetAllRepetitionTasksAsync(int userId)
        {
            return await _context.RepetitionTasks
                .Where(i => i.RepetitionSession.UserId == userId)
                .ToListAsync();
        }

        public async Task<RepetitionTask?> GetRepetitionTaskByIdAsync(int userId, int taskId)
        {
            return await _context.RepetitionTasks
                .Include(i => i.RepetitionSession)
                .FirstOrDefaultAsync(s => s.Id == taskId && s.RepetitionSession.UserId == userId);
        }

        public async Task<RepetitionState?> GetRepetitionStateByEntryIdAsync(int userId, int entryId)
        {
            return await _context.RepetitionStates
                .FirstOrDefaultAsync(r => r.UserId == userId && r.VocabularyEntryId == entryId);
        }



        public async Task<RequestResult<RepetitionSession>> StartRepetitionSessionAsync(int userId)
        {
            var user = await _accountService.GetByIdAsync(userId);

            if (user == null)
                return RequestResult<RepetitionSession>.Failure("USER_NOT_FOUND");


            if (user.RepetitionSession != null && user.RepetitionSession.InProccess)
                return RequestResult<RepetitionSession>.Failure("SESSION_NOT_FINISHED");

            else if (user.RepetitionSession != null && user.RepetitionSession.IsFinished)
                _context.RepetitionSessions.Remove(user.RepetitionSession);


            var entriesWithoutState = await _vocabularyService.GetAllEntriesWithoutStateAsync(userId);
            var states = entriesWithoutState.Select(e => new RepetitionState(user, e)).ToList();

            await _context.RepetitionStates.AddRangeAsync(states);


            var targetEntries = _vocabularyService.GetListOfRandomEntries(userId);
            var tasks = targetEntries.Select(e => new RepetitionTask(e, _random.Next(2) == 0)).ToList();

            var session = new RepetitionSession(user, tasks);

            await _context.RepetitionSessions.AddAsync(session);
            await _context.SaveChangesAsync();

            return RequestResult<RepetitionSession>.Success(session);
        }

        public async Task<RequestResult<RepetitionSessionResultDto>> FinishRepetitionSessionAsync(int userId)
        {
            var session = await GetRepetitionSessionAsync(userId);

            if (session == null)
                return RequestResult<RepetitionSessionResultDto>.Failure("SESSION_NOT_FOUND");

            if (session.Tasks == null || session.Tasks.Count == 0)
                return RequestResult<RepetitionSessionResultDto>.Failure("SESSION_HAS_NO_TASKS");


            if (!session.IsFinished)
            {
                session.FinishedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }


            var entriesIds = session.Tasks.Select(i => i.BaseVocabularyEntryId).ToList();
            var entriesDict = await _vocabularyService.GetEntriesDictByIdsAsync(userId, entriesIds);

            int missedCount = 0;
            var failedEntries = new List<VocabularyEntry>();

            foreach (var task in session.Tasks)
            {
                if (entriesDict.TryGetValue(task.BaseVocabularyEntryId, out var baseEntry) && baseEntry != null)
                {
                    var state = await AutoAssessmentRepetitionStateAsync(userId, task, baseEntry);

                    if (state.Value.IterationCounter == 0)
                        failedEntries.Add(baseEntry);
                }
                else
                {
                    missedCount++;
                }
            }


            int totalCount = session.Tasks.Count - missedCount;
            int correctCount = totalCount - failedEntries.Count;

            var result = new RepetitionSessionResultDto(
                correctCount,
                totalCount,
                Mapper.MapToDto(failedEntries),
                session.StartedAt,
                session.FinishedAt!.Value);

            return RequestResult<RepetitionSessionResultDto>.Success(result);
        }

        public async Task<RequestResult<RepetitionTask>> SubmitRepetitionTaskAnswerAsync(int userId, int taskId, string answer)
        {
            var task = await GetRepetitionTaskByIdAsync(userId, taskId);

            if (task == null)
                return RequestResult<RepetitionTask>.Failure("TASK_NOT_FOUND");

            if (task.RepetitionSession.IsFinished)
                return RequestResult<RepetitionTask>.Failure("SESSION_WAS_FINISHED");


            var currentTime     = DateTime.UtcNow;
            var lastActionTime  = task.RepetitionSession.LastActionAt;

            task.ActionCounter++;
            task.UserAnswer             = answer;
            task.ActionTimeSpan         = currentTime - lastActionTime;
            task.RepetitionSession.LastActionAt = currentTime;

            await _context.SaveChangesAsync();

            return RequestResult<RepetitionTask>.Success(task);
        }

        public async Task<RequestResult<RepetitionState>> SelfAssessmentRepetitionStateAsync(int userId, int entryId, double quality)
        {
            var state = await GetRepetitionStateByEntryIdAsync(userId, entryId);

            if (state == null)
                return RequestResult<RepetitionState>.Failure("REPETITION_STATE_NOT_FOUND");

            if (!state.CanSelfAssess)
                return RequestResult<RepetitionState>.Failure("REPETITION_STATE_ASSESS_NOT_ALLOWED");

            // Self features
            state.CanSelfAssess    = false;

            (int interval, double easinessFactor)
                = SM2Helper.NextIntervalAndEf(state.EasinessFactor, state.IterationInterval, state.IterationCounter, quality);

            state.IterationInterval = interval;
            state.EasinessFactor    = easinessFactor;

            await _context.SaveChangesAsync();

            return RequestResult<RepetitionState>.Success(state);
        }

        private async Task<RequestResult<RepetitionState>> AutoAssessmentRepetitionStateAsync(int userId, RepetitionTask task, VocabularyEntry entry)
        {
            double similarity = GetMaxAnswerSimilarity(task, entry);
            double quality = SM2Helper.ComputeQuality(task.RepetitionSession.AverageActionTime, task.ActionTimeSpan, task.ActionCounter, similarity);

            var state = await GetRepetitionStateByEntryIdAsync(userId, entry.Id);

            if (state == null)
                return RequestResult<RepetitionState>.Failure("REPETITION_STATE_NOT_FOUND");

            // Auto features
            state.IterationCounter  = SM2Helper.IsPassingQuality(quality) ? state.IterationCounter + 1 : 0;
            state.CanSelfAssess     = SM2Helper.IsPassingQuality(quality);
            state.LastRepetitionAt  = DateOnly.FromDateTime(DateTime.UtcNow);

            (int interval, double easinessFactor) 
                = SM2Helper.NextIntervalAndEf(state.EasinessFactor, state.IterationInterval, state.IterationCounter, quality);

            state.IterationInterval = interval;
            state.EasinessFactor    = easinessFactor;

            await _context.SaveChangesAsync();

            return RequestResult<RepetitionState>.Success(state);
        }

        private double GetMaxAnswerSimilarity(RepetitionTask task, VocabularyEntry entry)
        {
            string userAnswer = task.UserAnswer;

            if (task.IsForwardQuestion)
                return entry.Translations.Max(userAnswer.ComputeLevenshteinSimilarity);
            else
                return userAnswer.ComputeLevenshteinSimilarity(entry.Foreign);
        }
    }
}
