using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Itero.API.Data;
using Itero.API.Data.Entities;
using Itero.API.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Itero.API.Services
{
    public class VocabularyService
    {
        private AppDbContext _context;
        private UserService _userService;


        public VocabularyService(AppDbContext context, UserService userService)
        {
            _context = context;
            _userService = userService;
        }


        private IQueryable<VocabularyEntry> GetEntriesByUserQuery(int userId)
        {
            return _context.Entries.Where(e => e.User.Id == userId);
        }

        public async Task<List<VocabularyEntry>> GetAllEntriesAsync(int userId)
        {
            return await GetEntriesByUserQuery(userId)
                .ToListAsync();
        }

        public async Task<List<VocabularyEntry>> GetUserRandomEntriesAsync(int userId, int count=5)
        {
            return await GetEntriesByUserQuery(userId)
                .OrderBy(x => Guid.NewGuid())
                .Take(count)
                .ToListAsync();
        }


        public async Task<VocabularyEntry?> GetEntryByIdAsync(int userId, int id)
        {
            return await GetEntriesByUserQuery(userId)
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        }

        public async Task<VocabularyEntry?> GetEntryByKeyAsync(int userId, string key)
        {
            return await GetEntriesByUserQuery(userId)
                .FirstOrDefaultAsync(e => e.Foreign == key);
        }


        public async Task<VocabularyEntry?> CreateEntryAsync(int userId, VocabularyEntryDTO createDTO)
        {
            var user = _userService.GetById(userId);

            if (user == null)
                return null;


            var mapper = new VocabularyEntryMapper(user);
            string key = mapper.PrepareForeign(createDTO.Foreign);

            var currentEntry = await GetEntryByKeyAsync(userId, key);

            if (currentEntry != null)
                return null;


            var entry = mapper.Map(createDTO);

            await _context.Entries.AddAsync(entry);
            await _context.SaveChangesAsync();

            return entry;
        }

        public async Task<VocabularyEntry?> PatchEntryAsync(int userId, int entryId, VocabularyPatchDTO patchDTO)
        {
            var currentEntry = await GetEntryByIdAsync(userId, entryId);

            if (currentEntry == null)
                return null;


            var user = _userService.GetById(userId);

            if (user == null)
                return null;


            var mapper = new VocabularyEntryMapper(user);
            currentEntry = mapper.MapPatched(patchDTO, currentEntry);

            await _context.SaveChangesAsync();

            return currentEntry;
        }

        public async Task<bool> RemoveEntryById(int userId, int entryId)
        {
            var currentEntry = await GetEntryByIdAsync(userId, entryId);

            if (currentEntry == null)
                return false;
                

            _context.Entries.Remove(currentEntry);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
