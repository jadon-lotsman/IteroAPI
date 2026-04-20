using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mnemo.Contracts.Dtos.Vocabulary
{
    public class VocabularyEntryCreateDto
    {
        public string? Foreign { get; set; }
        public string? Transcription { get; set; }
        public string[]? Examples { get; set; }
        public string[]? Translations { get; set; }
    }
}
