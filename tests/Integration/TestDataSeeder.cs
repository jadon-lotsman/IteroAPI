using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mnemo.Common;
using Mnemo.Data;
using Mnemo.Data.Entities;

namespace tests.Integration
{
    public class TestDataSeeder
    {
        private AppDbContext _appDbContext;


        public TestDataSeeder(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }


        public User CreateUser(int id, string username)
        {
            var user = new User()
            {
                Id = id,
                Username = username
            };

            _appDbContext.Users.Add(user);
            _appDbContext.SaveChanges();
            return user;
        }

        public VocabularyEntry CreateEntry(int id, int userId,
            string foreign, params string[] translations)
        {
            var entry = new VocabularyEntry()
            {
                Id = id,
                Foreign = foreign,
                Transcription = string.Empty,
                Examples = [],
                Translations = translations.ToList(),
                UserId = userId
            };

            _appDbContext.Entries.Add(entry);
            _appDbContext.SaveChanges();
            return entry;
        }

        public RepetitionState CreateState(int id, int userId, int entryId,
            int repetitionCounter=0, int repetitionInterval=SM2Helper.MinInterval, double ef=SM2Helper.InitEF, bool canSelfAssess=false)
        {
            var state = new RepetitionState()
            {
                Id = id,
                IterationCounter = repetitionCounter,
                IterationInterval = repetitionInterval,
                EasinessFactor = ef,
                CanSelfAssess = canSelfAssess,
                LastRepetitionAt = DateOnly.FromDateTime(DateTime.UtcNow),
                UserId = userId,
                VocabularyEntryId = entryId
            };

            _appDbContext.RepetitionStates.Add(state);
            _appDbContext.SaveChanges();
            return state;
        }
    }
}
