using Mnemo.Contracts.Dtos.Vocabulary;
using Mnemo.Data.Entities;

namespace Mnemo.Contracts.Dtos.Repetition
{
    public class RepetitionResultResponse
    {
        public int Correct { get; set; }
        public int Total { get; set; }
        public int Percent { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime FinishedAt { get; set; }
        public VocabularyEntryResponse[]? VocabularyEntries { get; set; }
    }
}
