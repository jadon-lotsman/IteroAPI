using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Mnemo.Common;
using Mnemo.Contracts.Dtos.Vocabulary.Requests;
using Mnemo.Data;
using Mnemo.Data.Entities;
using Mnemo.Services.Queries;

namespace Mnemo.Services
{
    public class VocabularyManagementService
    {
        private AppDbContext _context;
        private AccountQueries _accountQueries;
        private VocabularyQueries _vocabularyQueries;


        public VocabularyManagementService(AppDbContext context, AccountQueries accountQueries, VocabularyQueries vocabularyQueries)
        {
            _context = context;
            _accountQueries = accountQueries;
            _vocabularyQueries = vocabularyQueries;
        }



        public async Task<RequestResult<VocabularyEntry>> CreateEntryAsync(int userId, CreateVocabularyEntryRequest dto)
        {
            if (!Mapper.ValidDto(dto))
                return RequestResult<VocabularyEntry>.Failure(ErrorCode.InvalidData);

            if (!await _accountQueries.ExistsByIdAsync(userId))
                return RequestResult<VocabularyEntry>.Failure(ErrorCode.UserNotFound);


            string foreign = Mapper.PrepareForeign(dto.Foreign!);

            if (await _vocabularyQueries.ExistsByForeignAsync(userId, foreign))
                return RequestResult<VocabularyEntry>.Failure(ErrorCode.DuplicateEntry);


            var entry = Mapper.MapToEntry(dto, userId);

            await _context.Entries.AddAsync(entry);
            await _context.SaveChangesAsync();

            return RequestResult<VocabularyEntry>.Success(entry);
        }

        public async Task<RequestResult<VocabularyEntry>> PatchEntryAsync(int userId, int entryId, PatchVocabularyEntryRequest patchDto)
        {
            if (!Mapper.ValidDto(patchDto))
                return RequestResult<VocabularyEntry>.Failure(ErrorCode.InvalidData);


            var currentEntry = await _vocabularyQueries.GetByIdAsync(userId, entryId);

            if (currentEntry == null)
                return RequestResult<VocabularyEntry>.Failure(ErrorCode.EntryNotFound);


            Mapper.PatchFromDto(currentEntry, patchDto);

            await _context.SaveChangesAsync();

            return RequestResult<VocabularyEntry>.Success(currentEntry);
        }

        public async Task<RequestResult<bool>> RemoveEntryByIdAsync(int userId, int entryId)
        {
            var currentEntry = await _vocabularyQueries.GetByIdAsync(userId, entryId);

            if (currentEntry == null)
                return RequestResult<bool>.Failure(ErrorCode.EntryNotFound);
                

            _context.Entries.Remove(currentEntry);
            await _context.SaveChangesAsync();

            return RequestResult<bool>.Success(true);
        }
    }
}
