using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Mnemo.Data;
using Mnemo.Services;

namespace tests.Integration
{
    public class IntegrationTestBase : IDisposable
    {
        protected IServiceProvider ServiceProvider { get; private set; }
        protected AppDbContext DbContext {  get; private set; }
        protected TestDataSeeder DataSeeder { get; private set; }


        public IntegrationTestBase()
        {
            var services = new ServiceCollection();

            var dbName = Guid.NewGuid().ToString();
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));

            services.AddScoped<AccountManagementService>();
            services.AddScoped<RepetitionSessionService>();
            services.AddScoped<RepetitionStateService>();
            services.AddScoped<VocabularyManagementService>();

            ServiceProvider = services.BuildServiceProvider();
            DbContext = ServiceProvider.GetRequiredService<AppDbContext>();
            DataSeeder = new TestDataSeeder(DbContext);
        }


        public void Dispose()
        {
            DbContext.Database.EnsureDeleted();
            DbContext.Dispose();
        }
    }
}
