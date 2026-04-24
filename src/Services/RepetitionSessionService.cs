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
using Mnemo.Services.Handlers;
using Mnemo.Services.Queries;

namespace Mnemo.Services
{
    public class RepetitionSessionService
    {
        private AppDbContext _context;
        private AccountQueries _accountQueries;
        private SessionQueries _sessionQueries;
        private VocabularyQueries _vocabularyQueries;

        private RepetitionStateService _stateService;
        private RepetitionResultHandler _resultHandler;

        private static Random _random = new Random();


        public RepetitionSessionService(AppDbContext context, AccountQueries accountQueries, SessionQueries sessionQueries, VocabularyQueries vocabularyQueries, RepetitionStateService stateService, RepetitionResultHandler resultHandler)
        {
            _context = context;

            _accountQueries = accountQueries;
            _sessionQueries = sessionQueries;
            _vocabularyQueries = vocabularyQueries;

            _stateService = stateService;
            _resultHandler = resultHandler;
        }



        public async Task<RequestResult<RepetitionSession>> GetRepetitionSessionStatusAsync(int userId)
        {
            if (!await _accountQueries.ExistsByIdAsync(userId))
                return RequestResult<RepetitionSession>.Failure(ErrorCode.UserNotFound);

            var session = await _sessionQueries.GetByUserIdAsync(userId);

            if (session == null)    return RequestResult<RepetitionSession>.Failure(ErrorCode.SessionNotFound);
            else return RequestResult<RepetitionSession>.Failure(ErrorCode.SessionNotFinished);
        }



        public async Task<RequestResult<RepetitionSession>> StartRepetitionSessionAsync(int userId)
        {
            if (!await _accountQueries.ExistsByIdAsync(userId))
                return RequestResult<RepetitionSession>.Failure(ErrorCode.UserNotFound);

            if (await _sessionQueries.ExistsByUserId(userId))
                return RequestResult<RepetitionSession>.Failure(ErrorCode.SessionNotFinished);


            await _stateService.RefreshRepetitionStatesAsync(userId);


            var targetEntries = _vocabularyQueries.GetRandomByUserId(userId);
            var tasks = targetEntries.Select(e => new RepetitionTask(e, _random.Next(2) == 0)).ToList();

            var session = new RepetitionSession(userId, tasks);

            await _context.RepetitionSessions.AddAsync(session);
            await _context.SaveChangesAsync();

            return RequestResult<RepetitionSession>.Success(session);
        }

        public async Task<RequestResult<RepetitionResult>> FinishRepetitionSessionAsync(int userId)
        {
            var session = await _sessionQueries.GetByUserIdAsync(userId);

            if (session == null)
                return RequestResult<RepetitionResult>.Failure(ErrorCode.SessionNotFound);


            var tasks = await _sessionQueries.GetTasksByUserIdAsync(userId);

            //if (tasks.Count == 0)
            //    return ServiceResult<RepetitionResult>.Failure(ServiceErrorCode.SessionNoTasks);


            session.FinishedAt = DateTime.UtcNow;

            RepetitionResult result = await _resultHandler.ExecuteAsync(userId, tasks);

            result.StartedAt = session.StartedAt;
            result.FinishedAt = session.FinishedAt.Value;

            _context.RepetitionResults.Add(result);
            _context.RepetitionSessions.Remove(session);
            _context.RepetitionTasks.RemoveRange(tasks);
            await _context.SaveChangesAsync();

            return RequestResult<RepetitionResult>.Success(result);
        }

        public async Task<RequestResult<RepetitionTask>> SubmitRepetitionTaskAnswerAsync(int userId, int taskId, string answer)
        {
            var task = await _sessionQueries.GetTaskByIdAsync(userId, taskId);

            if (task == null)
                return RequestResult<RepetitionTask>.Failure(ErrorCode.TaskNotFound);


            var currentTime     = DateTime.UtcNow;
            var lastActionTime  = task.RepetitionSession.LastActionAt;

            task.ActionCounter++;
            task.UserAnswer             = answer;
            task.ActionTimeSpan         = currentTime - lastActionTime;
            task.RepetitionSession.LastActionAt = currentTime;

            await _context.SaveChangesAsync();

            return RequestResult<RepetitionTask>.Success(task);
        }
    }
}
