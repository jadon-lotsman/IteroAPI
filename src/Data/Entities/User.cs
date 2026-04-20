using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mnemo.Data.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; }
        public DateTime RegisteredAt { get; set; }


        public RepetitionSession? RepetitionSession { get; set; }
        public List<RepetitionState> RepetitionStates { get; set; }
        public List<VocabularyEntry> VocabularyEntries { get; set; }


        public User() {}

        public User(string username)
        {
            Username        = username;
            RegisteredAt    = DateTime.UtcNow;

            RepetitionStates = new List<RepetitionState>();
            VocabularyEntries = new List<VocabularyEntry>();
        }
    }
}
