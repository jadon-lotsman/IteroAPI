using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Mnemo.Common;
using Mnemo.Contracts.Dtos.Vocabulary;
using Mnemo.Data;
using Mnemo.Data.Entities;

namespace Mnemo.Services
{
    public class VocabularyManagementService
    {
        private AppDbContext _context;
        private AccountManagementService _accountService;


        public VocabularyManagementService(AppDbContext context, AccountManagementService accountService)
        {
            _context = context;
            _accountService = accountService;
        }


        private IQueryable<VocabularyEntry> GetEntriesByUserQuery(int userId)
        {
            return _context.Entries.Where(e => e.User.Id == userId);
        }

        public async Task<VocabularyEntry?> GetEntryByIdAsync(int userId, int id)
        {
            return await GetEntriesByUserQuery(userId)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<VocabularyEntry?> GetEntryByKeyAsync(int userId, string key)
        {
            return await GetEntriesByUserQuery(userId)
                .FirstOrDefaultAsync(e => string.Equals(e.Foreign, key));
        }

        public async Task<List<VocabularyEntry>> GetAllEntriesAsync(int userId)
        {
            return await GetEntriesByUserQuery(userId)
                .ToListAsync();
        }

        public async Task<Dictionary<int, VocabularyEntry>> GetEntriesDictByIdsAsync(int userId, IEnumerable<int> ids)
        {
            var list = await GetEntriesByUserQuery(userId)
                .Where(e => ids.Contains(e.Id))
                .ToListAsync();

            return list.ToDictionary(e => e.Id);
        }


        public List<VocabularyEntry> GetListOfRandomEntries(int userId, int count=5)
        {
            return _context.Entries.Where(e => e.User.Id == userId)
                .AsEnumerable()
                .OrderBy(x => Guid.NewGuid())
                .Take(count)
                .ToList();
        }

        public async Task<List<VocabularyEntry>> GetListOfDueEntries(int userId, int count=5)
        {
            return await _context.Entries.Where(e => e.User.Id == userId)
                .Include(e => e.RepetitionState)
                .Where(e => e.RepetitionState.NextRepetitionAt <= DateOnly.FromDateTime(DateTime.UtcNow))
                .Take(count)
                .ToListAsync();
        }



        public async Task<RequestResult<VocabularyEntry>> CreateEntryAsync(int userId, VocabularyEntryCreateDto dto)
        {
            if (!Mapper.ValidDto(dto))
                return RequestResult<VocabularyEntry>.Failure("INVALID_DATA");


            var user = await _accountService.GetByIdAsync(userId);

            if (user == null)
                return RequestResult<VocabularyEntry>.Failure("USER_NOT_FOUND");


            string foreignKey = Mapper.PrepareForeign(dto.Foreign!);
            var entryByKey = await GetEntryByKeyAsync(userId, foreignKey);

            if (entryByKey != null)
                return RequestResult<VocabularyEntry>.Failure("DUPLICATE_ENTRY");


            var entry = Mapper.MapToEntry(dto, user);

            await _context.Entries.AddAsync(entry);
            await _context.SaveChangesAsync();

            return RequestResult<VocabularyEntry>.Success(entry);
        }

        public async Task<RequestResult<VocabularyEntry>> PatchEntryAsync(int userId, int entryId, VocabularyEntryPatchDto patchDto)
        {
            if (!Mapper.ValidDto(patchDto))
                return RequestResult<VocabularyEntry>.Failure("INVALID_DATA");


            var currentEntry = await GetEntryByIdAsync(userId, entryId);

            if (currentEntry == null)
                return RequestResult<VocabularyEntry>.Failure("ENTRY_NOT_FOUND");


            Mapper.PatchFromDto(currentEntry, patchDto);

            await _context.SaveChangesAsync();

            return RequestResult<VocabularyEntry>.Success(currentEntry);
        }

        public async Task<RequestResult<bool>> RemoveEntryByIdAsync(int userId, int entryId)
        {
            var currentEntry = await GetEntryByIdAsync(userId, entryId);

            if (currentEntry == null)
                return RequestResult<bool>.Failure("ENTRY_NOT_FOUND");
                

            _context.Entries.Remove(currentEntry);
            await _context.SaveChangesAsync();

            return RequestResult<bool>.Success(true);
        }
    }
}
