using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Itero.API.Data;
using Itero.API.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Itero.API.Services
{
    public class IterationService
    {
        private AppDbContext _context;
        private VocabularyService _vocabularyService;
        private UserService _userService;

        public IterationService(AppDbContext context, UserService userService, VocabularyService vocabularyService)
        {
            _context = context;
            _userService = userService;
            _vocabularyService = vocabularyService;
        }


        public async Task<bool> GetAnyAsync(int userId)
        {
            return await _context.Iterations
                .AnyAsync(e => e.User.Id == userId);
        }

        public async Task<Iteration?> GetIterationAsync(int userId)
        {
            return await _context.Iterations
                .FirstOrDefaultAsync(e => e.User.Id == userId);
        }

        public async Task<IterationStep?> GetIterationStepByIdASync(int userId, int stepId)
        {
            return await _context.Questions
                .FirstOrDefaultAsync(s => s.Id == stepId && s.Iteration.UserId == userId);
        }

        public async Task<Iteration?> CreateIterationAsync(int userId)
        {
            bool HasAny = await GetAnyAsync(userId);

            if (HasAny)
                return null;
             

            var stepsList = new List<IterationStep>();
            var rendomEntries = _vocabularyService.GetUserRandomEntriesAsync(userId).Result;

            foreach (var entry in rendomEntries)
                stepsList.Add(new IterationStep(entry, true));

            var currentUser = _userService.GetById(userId);
            var currentIteration = new Iteration(currentUser, stepsList);

            _context.Iterations.Add(currentIteration);
            _context.SaveChanges();

            return currentIteration;
        }


        public async Task<bool> SetStepValueAsync(int userId, int stepId, string userValue)
        {
            var iterationPart = await GetIterationStepByIdASync(userId, stepId);

            if (iterationPart == null)
                return false;
                
            iterationPart.UserValue = userValue;

            _context.SaveChanges();

            return true;
        }

        public bool GetResult(int userId)
        {
            // TODO: Сформировать IterationResult и вернуть его
            return true;
        }
    }
}
