using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mnemo.Data.Entities
{
    public class RepetitionSession
    {
        public int Id { get; set; }

        public bool InProccess => !FinishedAt.HasValue;
        public bool IsFinished => FinishedAt.HasValue;
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public DateTime LastActionAt { get; set; }
        public TimeSpan AverageActionTime
        {
            get
            {
                var sum = TimeSpan.Zero;
                Tasks.Select(i => sum.Add(i.ActionTimeSpan));
                return sum / Tasks.Count;
            }
        }


        public int UserId { get; set; }
        public User User { get; set; }
        public List<RepetitionTask> Tasks { get; set; }


        public RepetitionSession() { }

        public RepetitionSession(User user, List<RepetitionTask> tasks)
        {
            StartedAt       = DateTime.UtcNow;
            LastActionAt    = StartedAt;
            FinishedAt      = null;

            User = user;
            Tasks = tasks;
        }
    }
}
