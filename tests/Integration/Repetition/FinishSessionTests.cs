using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mnemo.Common;
using Mnemo.Data.Entities;
using Mnemo.Services;

namespace tests.Integration.Repetition
{
    public class FinishSessionTests : IntegrationTestBase
    {
        [Fact]
        public async Task FinishSession_ShouldStopSession()
        {
            // Arrange
            var user = DataSeeder.CreateUser(id: 3, username: "Bob");
            var entry = DataSeeder.CreateEntry(id: 7, userId: user.Id, foreign: "apple", translations: "яблоко");
            var state = DataSeeder.CreateState(id: 1, userId: user.Id, entryId: entry.Id, repetitionCounter: 2, repetitionInterval: 4, ef: SM2Helper.InitEF);

            var existingSession = new RepetitionSession(user, new List<RepetitionTask>() { new RepetitionTask(entry, true) });

            DbContext.RepetitionSessions.Add(existingSession);
            await DbContext.SaveChangesAsync();

            var repetitionService = ServiceProvider.GetRequiredService<RepetitionSessionService>();


            // Act
            var result = await repetitionService.FinishRepetitionSessionAsync(user.Id);


            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(existingSession.IsFinished);
            Assert.True(existingSession.StartedAt < existingSession.FinishedAt);
        }
    }
}
