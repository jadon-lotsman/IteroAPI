using Itereta.Data.Entities;
using Mnemo.Contracts.Dtos.Vocabulary;

namespace Mnemo.Contracts.Dtos.Repetition
{
    public class RepetitionSessionResultDto
    {
        public int Correct { get; set; }
        public int Total { get; set; }
        public float Percent { get; set; }
        public char LetterGrade { get; set; }
        public DateTime Started { get; set; }
        public DateTime Finished { get; set; }
        public VocabularyEntryResponseDto[] FailedEntries { get; set; }


        public RepetitionSessionResultDto(int correct, int total, VocabularyEntryResponseDto[] failedEntries, DateTime started, DateTime finished)
        {
            Correct = correct;
            Total = total;
            Percent = Total == 0? 0 : (float) Correct / Total * 100;
            Started = started;
            Finished = finished;
            FailedEntries = failedEntries;

            if (Percent >= 90)
                LetterGrade = 'A';
            else if (Percent >= 80)
                LetterGrade = 'B';
            else if (Percent >= 70)
                LetterGrade = 'C';
            else if (Percent >= 60)
                LetterGrade = 'D';
            else
                LetterGrade = 'F';
        }
    }
}
