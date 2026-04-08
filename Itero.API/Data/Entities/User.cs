using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itero.API.Data.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public Iteration? Iteration { get; set; }
        public List<VocabularyEntry> Entries { get; set; }
        public DateTime Registered { get; set; }


        public User() {}

        public User(string username)
        {
            Username = username;
            Entries = new List<VocabularyEntry>();

            Registered = DateTime.UtcNow;
        }
    }
}
