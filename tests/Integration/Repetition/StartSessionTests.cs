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
    public class StartSessionTests : IntegrationTestBase
    {
        [Fact]
        public async Task StartSession_WhenNoActiveSession_ShouldCreateNewSessionAndStates()
        {
            // Arrange
            var user = DataSeeder.CreateUser(id: 3, username: "Bob");
            var entry1 = DataSeeder.CreateEntry(id: 7, userId: user.Id, foreign: "apple", translations: "яблоко");
            var entry2 = DataSeeder.CreateEntry(id: 8, userId: user.Id, foreign: "banana", translations: "банан");

            var sessionService = ServiceProvider.GetRequiredService<RepetitionSessionService>();


            // Act
            var result = await sessionService.StartRepetitionSessionAsync(user.Id);


            // Assert
            Assert.True(result.IsSuccess);
            var session = result.Value;

            Assert.NotNull(session);
            Assert.True(session.InProccess);
            Assert.Equal(2, session.Tasks.Count);
        }


        [Fact]
        public async Task StartSession_WhenActiveSessionExists_ShouldReturnFailure()
        {
            // Arrange
            var user = DataSeeder.CreateUser(id: 3, username: "Bob");
            var existingSession = new RepetitionSession(user, new List<RepetitionTask>());

            DbContext.RepetitionSessions.Add(existingSession);
            await DbContext.SaveChangesAsync();

            var sessionService = ServiceProvider.GetRequiredService<RepetitionSessionService>();


            // Act
            var result = await sessionService.StartRepetitionSessionAsync(user.Id);


            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("SESSION_NOT_FINISHED", result.ErrorCode);
        }
    }
}
