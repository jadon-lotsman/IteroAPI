using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Mnemo.Data.Entities;

namespace Mnemo.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<VocabularyEntry> Entries { get; set; }

        public DbSet<RepetitionSession> RepetitionSessions { get; set; }
        public DbSet<RepetitionState> RepetitionStates { get; set; }
        public DbSet<RepetitionTask> RepetitionTasks { get; set; }
        

        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }
    }
}
