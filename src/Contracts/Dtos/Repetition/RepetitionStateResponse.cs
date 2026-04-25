using Mnemo.Contracts.Dtos.Vocabulary;

namespace Mnemo.Contracts.Dtos.Repetition
{
    public class RepetitionStateResponse
    {
        public int Id { get; set; }
        public int RepetitionCounter { get; set; }
        public int RepetitionInterval { get; set; }
        public double EasinessFactor { get; set; }
        public bool CanSelfAssess { get; set; }
        public DateOnly NextRepetitionAt { get; set; }
        public VocabularyEntryResponse VocabularyEntry { get; set; }
    }
}
