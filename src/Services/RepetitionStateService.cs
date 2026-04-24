using Microsoft.EntityFrameworkCore;
using Mnemo.Common;
using Mnemo.Data;
using Mnemo.Data.Entities;
using Mnemo.Services.Queries;

namespace Mnemo.Services
{
    public class RepetitionStateService
    {
        private AppDbContext _context;
        private StateQueries _stateQueries;


        public RepetitionStateService(AppDbContext context, StateQueries stateQueries)
        {
            _context = context;
            _stateQueries = stateQueries;
        }



        public async Task<RequestResult<bool>> RefreshRepetitionStatesAsync(int userId)
        {
            var entries = await _stateQueries.GetEntriesWithoutRepetitionStateAsync(userId);

            if (!entries.Any())
                return RequestResult<bool>.Success(false);


            var states = entries.Select(e => new RepetitionState(userId, e)).ToList();
            await _context.RepetitionStates.AddRangeAsync(states);

            return RequestResult<bool>.Success(true);
        }


        public async Task<RequestResult<RepetitionState>> UpdateRepetitionStateAsync(int userId, int entryId, double quality, bool shouldIncrementCounter)
        {
            var state = await _stateQueries.GetByEntryIdAsync(userId, entryId);

            if (state == null)
                return RequestResult<RepetitionState>.Failure(ErrorCode.StateNotFound);


            if (shouldIncrementCounter)
            {
                state.IterationCounter = SM2Helper.IsPassingQuality(quality) ? state.IterationCounter + 1 : 0;
                state.CanSelfAssess = SM2Helper.IsPassingQuality(quality);
                state.LastRepetitionAt = DateOnly.FromDateTime(DateTime.UtcNow);
            }
            else
            {
                if (!state.CanSelfAssess)
                    return RequestResult<RepetitionState>.Failure(ErrorCode.ActionNotAllowed);

                state.CanSelfAssess = false;
            }


            (int interval, double easinessFactor)
                = SM2Helper.NextIntervalAndEf(state.EasinessFactor, state.IterationInterval, state.IterationCounter, quality);

            state.IterationInterval = interval;
            state.EasinessFactor = easinessFactor;


            await _context.SaveChangesAsync();

            return RequestResult<RepetitionState>.Success(state);
        }
    }
}
