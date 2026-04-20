using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mnemo.Data.Entities
{
    public class RepetitionTask
    {
        public int Id { get; set; }
        public int BaseVocabularyEntryId { get; set; }

        public string Prompt { get; set; }
        public string UserAnswer { get; set; }
        public bool IsForwardQuestion { get; set; }
        public int ActionCounter { get; set; }
        public TimeSpan ActionTimeSpan { get; set; }


        public int RepetitionSessionId { get; set; }
        public RepetitionSession RepetitionSession { get; set; }


        public RepetitionTask() { }

        public RepetitionTask(VocabularyEntry entry, bool isForwardQuestion)
        {
            BaseVocabularyEntryId = entry.Id;

            Prompt = isForwardQuestion ? entry.Foreign : entry.Translations[0];
            UserAnswer = string.Empty;
            IsForwardQuestion = isForwardQuestion;
        }
    }
}
