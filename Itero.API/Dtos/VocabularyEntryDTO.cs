using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itero.API.Dtos
{
    public class VocabularyEntryDTO
    {
        public string Foreign { get; set; }
        public string Transcription { get; set; }
        public string[] Examples { get; set; }
        public string[] Translations { get; set; }
    }
}
