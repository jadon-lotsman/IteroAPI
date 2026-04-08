using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itero.API.Data.Entities
{
    public class VocabularyEntry
    {
        public int Id { get; set; }
        public string Foreign { get; set; }
        public string Transcription { get; set; }
        public List<string> Examples { get; set; }
        public List<string> Translations { get; set; }
        public DateTime Created { get; set; }



        public int UserId { get; set; }
        public User User { get; set; }


        public VocabularyEntry()
        {
            Examples = new List<string>();
            Translations = new List<string>();

            Created = DateTime.UtcNow;
        }
    }
}
