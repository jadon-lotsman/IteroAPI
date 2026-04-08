using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itero.API.Data.Entities
{
    public class Iteration
    {
        public int Id { get; set; }
        public bool InProcess { get; set; }
        public List<IterationStep>? Questions { get; set; }
        public DateTime Created { get; set; }


        public int UserId { get; set; }
        public User User { get; set; }


        public Iteration() { }

        public Iteration(User user, List<IterationStep> questions)
        {
            InProcess = true;

            Questions = questions;
            User = user;

            Created = DateTime.UtcNow;
        }
    }
}
