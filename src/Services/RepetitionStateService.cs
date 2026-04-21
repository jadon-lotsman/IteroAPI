using Microsoft.EntityFrameworkCore;
using Mnemo.Common;
using Mnemo.Data;
using Mnemo.Data.Entities;

namespace Mnemo.Services
{
    public class RepetitionStateService
    {
        private AppDbContext _context;
        private AccountManagementService _accountService;
        private VocabularyManagementService _vocabularyService;


        public RepetitionStateService(AppDbContext context, AccountManagementService accountService, VocabularyManagementService vocabularyService)
        {
            _context = context;
            _accountService = accountService;
            _vocabularyService = vocabularyService;
        }

        public IQueryable<RepetitionState> GetRepetitionStatesByUserQuery(int userId)
        {
            return _context.RepetitionStates.Where(s => s.UserId == userId);
        }

        public async Task<RepetitionState?> GetRepetitionStateByEntryIdAsync(int userId, int entryId)
        {
            return await GetRepetitionStatesByUserQuery(userId)
                .FirstOrDefaultAsync(s => s.VocabularyEntryId == entryId);
        }

        public async Task<List<RepetitionState>> GetAllRepetitionStatesAsync(int userId)
        {
            return await GetRepetitionStatesByUserQuery(userId)
                .ToListAsync();
        }


        public async Task<List<VocabularyEntry>> GetEntriesWithoutRepetitionStateAsync(int userId)
        {
            return await _context.Entries.Where(e => e.User.Id == userId)
                .Where(e => e.RepetitionState == null)
                .ToListAsync();
        }


        public async Task<RequestResult<bool>> RefreshRepetitionStatesAsync(int userId)
        {
            var entries = await GetEntriesWithoutRepetitionStateAsync(userId);

            if (!entries.Any())
                return RequestResult<bool>.Success(false);


            var states = entries.Select(e => new RepetitionState(e.User, e)).ToList();
            await _context.RepetitionStates.AddRangeAsync(states);

            return RequestResult<bool>.Success(true);
        }


        public async Task<RequestResult<RepetitionState>> UpdateRepetitionStateAsync(int userId, int entryId, double quality, bool shouldIncrementCounter)
        {
            var state = await GetRepetitionStateByEntryIdAsync(userId, entryId);

            if (state == null)
                return RequestResult<RepetitionState>.Failure("REPETITION_STATE_NOT_FOUND");


            if (shouldIncrementCounter)
            {
                state.IterationCounter = SM2Helper.IsPassingQuality(quality) ? state.IterationCounter + 1 : 0;
                state.CanSelfAssess = SM2Helper.IsPassingQuality(quality);
                state.LastRepetitionAt = DateOnly.FromDateTime(DateTime.UtcNow);
            }
            else
            {
                if (!state.CanSelfAssess)
                    return RequestResult<RepetitionState>.Failure("REPETITION_STATE_ASSESS_NOT_ALLOWED");

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
