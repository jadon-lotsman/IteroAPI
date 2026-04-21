using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Mnemo.Common;
using Mnemo.Services;

namespace tests.Integration.Repetition
{
    public class SelfAssesmentTests : IntegrationTestBase
    {
        [Fact]
        public async Task SelfAssessment_WhenAllowed_ShouldUpdateStateAndDisableFlag()
        {
            // Arrange
            var user  = DataSeeder.CreateUser (id: 3, username: "Bob");
            var entry = DataSeeder.CreateEntry(id: 7, userId: user.Id, foreign: "apple", translations: "яблоко");

            var state = DataSeeder.CreateState(id: 1, userId: user.Id, entryId: entry.Id, repetitionCounter: 2, repetitionInterval: 4, ef: SM2Helper.InitEF);
            state.CanSelfAssess = true;

            var stateService = ServiceProvider.GetRequiredService<RepetitionStateService>();


            // Act
            var result = await stateService.UpdateRepetitionStateAsync(userId: user.Id, entryId: entry.Id, quality: 5, shouldIncrementCounter: false);
            

            // Assert
            Assert.True(result.IsSuccess);
            var updatedState = result.Value!;

            Assert.False(updatedState.CanSelfAssess);
            Assert.True(updatedState.EasinessFactor > SM2Helper.InitEF);
            Assert.True(updatedState.IterationInterval > 4);
        }

        [Fact]
        public async Task SelfAssessment_WhenNotAllowed_ShouldReturnFailure()
        {
            // Arrange
            var user  = DataSeeder.CreateUser (id: 3, username: "Bob");
            var entry = DataSeeder.CreateEntry(id: 7, userId: user.Id, foreign: "apple", translations: "яблоко");

            var state = DataSeeder.CreateState(id: 1, userId: user.Id, entryId: entry.Id, repetitionCounter: 2, repetitionInterval: 4, ef: SM2Helper.InitEF);
            state.CanSelfAssess = false;

            var stateService = ServiceProvider.GetRequiredService<RepetitionStateService>();


            // Act
            var result = await stateService.UpdateRepetitionStateAsync(userId: user.Id, entryId: entry.Id, quality: 5, shouldIncrementCounter: false);


            // Assert
            Assert.False(result.IsSuccess);
            var updatedState = result.Value!;
        }
    }
}
