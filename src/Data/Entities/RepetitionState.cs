using Mnemo.Common;

namespace Mnemo.Data.Entities
{
    public class RepetitionState
    {
        public int Id { get; set; }

        public int IterationCounter { get; set; }
        public int IterationInterval { get; set; }
        public double EasinessFactor { get; set; }
        public bool CanSelfAssess { get; set; }
        public DateOnly LastRepetitionAt { get; set; }
        public DateOnly NextRepetitionAt => LastRepetitionAt.AddDays(IterationInterval);


        public int UserId { get; set; }
        public User User { get; set; }
        public int VocabularyEntryId { get; set; }
        public VocabularyEntry VocabularyEntry { get; set; }


        public RepetitionState() { }

        public RepetitionState(User user, VocabularyEntry entry)
        {
            IterationCounter    = 0;
            IterationInterval   = SM2Helper.MinInterval;
            EasinessFactor      = SM2Helper.InitEF;
            CanSelfAssess       = false;
            LastRepetitionAt    = DateOnly.FromDateTime(DateTime.UtcNow);

            User = user;
            VocabularyEntry = entry;
        }
    }
}
