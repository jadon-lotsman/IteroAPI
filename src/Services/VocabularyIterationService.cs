using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Mnemo.Common;
using Mnemo.Contracts.Dtos.Repetition;
using Mnemo.Data;
using Mnemo.Data.Entities;

namespace Mnemo.Services
{
    public class VocabularyIterationService
    {
        private AppDbContext _context;
        private AccountService _accountService;
        private VocabularyManagementService _vocabularyService;

        private static Random _random = new Random();


        public VocabularyIterationService(AppDbContext context, AccountService userService, VocabularyManagementService vocabularyService)
        {
            _context = context;
            _accountService = userService;
            _vocabularyService = vocabularyService;
        }


        public async Task<RequestResult<RepetitionSession>> GetIterationStatusAsync(int userId)
        {
            var iteration = await GetIterationAsync(userId);
            if (iteration == null) return RequestResult<RepetitionSession>.Failure("ITERATION_NOT_FOUND");
            if (iteration.IsFinished) return RequestResult<RepetitionSession>.Failure("ITERATION_WAS_FINISHED");
            else return RequestResult<RepetitionSession>.Failure("ITERATION_IN_PROCESS");
        }

        public async Task<RepetitionSession?> GetIterationAsync(int userId)
        {
            return await _context.RepetitionSessions
                .Include(i => i.Tasks)
                .FirstOrDefaultAsync(e => e.User.Id == userId);
        }

        public async Task<List<RepetitionTask>> GetAllIterettesAsync(int userId)
        {
            return await _context.RepetitionTasks
                .Where(i => i.RepetitionSession.UserId == userId)
                .ToListAsync();
        }

        public async Task<RepetitionTask?> GetIteretteByIdAsync(int userId, int iteretteId)
        {
            return await _context.RepetitionTasks
                .Include(i => i.RepetitionSession)
                .FirstOrDefaultAsync(s => s.Id == iteretteId && s.RepetitionSession.UserId == userId);
        }

        public async Task<RepetitionState?> GetRepetitionStateByEntryIdAsync(int userId, int entryId)
        {
            return await _context.RepetitionStates
                .FirstOrDefaultAsync(r => r.UserId == userId && r.VocabularyEntryId == entryId);
        }



        public async Task<RequestResult<RepetitionSession>> StartIterationAsync(int userId)
        {
            var user = await _accountService.GetByIdAsync(userId);

            if (user == null)
                return RequestResult<RepetitionSession>.Failure("USER_NOT_FOUND");


            if (user.RepetitionSession != null && user.RepetitionSession.InProccess)
                return RequestResult<RepetitionSession>.Failure("ITERATION_NOT_FINISHED");

            else if (user.RepetitionSession != null && user.RepetitionSession.IsFinished)
                _context.RepetitionSessions.Remove(user.RepetitionSession);

            var entriesWithoutState = await _vocabularyService.GetAllEntriesWithoutStateAsync(userId);

            List<RepetitionState> states = entriesWithoutState
                .Select(e => new RepetitionState(user, e)).ToList();

            await _context.RepetitionStates.AddRangeAsync(states);


            var targetEntries = _vocabularyService.GetListOfRandomEntries(userId);
            List<RepetitionTask> iterettes = targetEntries
                .Select(e => new RepetitionTask(e, _random.Next(2) == 0)).ToList();


            var iteration = new RepetitionSession(user, iterettes);

            await _context.RepetitionSessions.AddAsync(iteration);
            await _context.SaveChangesAsync();

            return RequestResult<RepetitionSession>.Success(iteration);
        }

        public async Task<RequestResult<RepetitionSessionResultDto>> FinishIterationAsync(int userId)
        {
            var iteration = await GetIterationAsync(userId);

            if (iteration == null)
                return RequestResult<RepetitionSessionResultDto>.Failure("ITERATION_NOT_FOUND");

            if (iteration.Tasks == null)
                return RequestResult<RepetitionSessionResultDto>.Failure("ITERATION_HAS_NO_ITERETTES");


            if (!iteration.IsFinished)
            {
                iteration.FinishedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }


            var entriesIds = iteration.Tasks
                .Select(i => i.BaseVocabularyEntryId).ToList();
            var entriesDict = await _vocabularyService.GetEntriesDictByIdsAsync(userId, entriesIds);

            int missedCount = 0;
            var failedEntries = new List<VocabularyEntry>();

            foreach (var iterette in iteration.Tasks)
            {
                if (entriesDict.TryGetValue(iterette.BaseVocabularyEntryId, out var baseEntry) && baseEntry != null)
                {
                    var state = await AutoAssessmentAsync(userId, iterette, baseEntry);

                    //if (state.Value.IterationCounter!=0)
                    //    failedEntries.Add(baseEntry);
                }
                else
                {
                    missedCount++;
                }
            }


            int totalCount = iteration.Tasks.Count - missedCount;
            int correctCount = totalCount - failedEntries.Count;

            var result = new RepetitionSessionResultDto(
                correctCount,
                totalCount,
                Mapper.MapToDto(failedEntries),
                iteration.StartedAt,
                iteration.FinishedAt!.Value);

            return RequestResult<RepetitionSessionResultDto>.Success(result);
        }

        public async Task<RequestResult<RepetitionTask>> SubmitIteretteAnswerAsync(int userId, int iteretteId, string answer)
        {
            var task = await GetIteretteByIdAsync(userId, iteretteId);

            if (task == null)
                return RequestResult<RepetitionTask>.Failure("ITERETTE_NOT_FOUND");

            if (task.RepetitionSession.IsFinished)
                return RequestResult<RepetitionTask>.Failure("ITERATION_WAS_FINISHED");


            var currentTime     = DateTime.UtcNow;
            var lastActionTime  = task.RepetitionSession.LastActionAt;

            task.ActionCounter++;
            task.UserAnswer             = answer;
            task.ActionTimeSpan         = currentTime - lastActionTime;
            task.RepetitionSession.LastActionAt = currentTime;

            await _context.SaveChangesAsync();

            return RequestResult<RepetitionTask>.Success(task);
        }

        public async Task<RequestResult<RepetitionState>> SelfAssessmentAsync(int userId, int entryId, double quality)
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

        private async Task<RequestResult<RepetitionState>> AutoAssessmentAsync(int userId, RepetitionTask iterette, VocabularyEntry entry)
        {
            double similarity = GetMaxAnswerSimilarity(iterette, entry);
            double quality = SM2Helper.ComputeQuality(iterette.RepetitionSession.AverageActionTime, iterette.ActionTimeSpan, iterette.ActionCounter, similarity);

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

        private double GetMaxAnswerSimilarity(RepetitionTask iterette, VocabularyEntry entry)
        {
            string userAnswer = iterette.UserAnswer;

            if (iterette.IsForwardQuestion)
                return entry.Translations.Max(userAnswer.ComputeLevenshteinSimilarity);
            else
                return userAnswer.ComputeLevenshteinSimilarity(entry.Foreign);
        }
    }
}
