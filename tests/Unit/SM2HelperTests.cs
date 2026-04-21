using Mnemo.Common;

namespace tests.Unit
{
    public class SM2HelperTests
    {
        [Theory]
        [InlineData(10, 10, 1, 1.0, true)]
        [InlineData(10, 12, 1, 0.9, true)]
        [InlineData(10, 10, 3, 0.85, true)]
        [InlineData(10, 20, 1, 0.3, false)]
        [InlineData(10, 10, 1, 0.6, false)]
        public void ComputeQuality_ShouldReturnCorrectPassing(double averageSec, double actionSec, int actionCounter, double similarity, bool expectedPassing)
        {
            double quality = SM2Helper.ComputeQuality(
                TimeSpan.FromSeconds(averageSec),
                TimeSpan.FromSeconds(actionSec),
                actionCounter,
                similarity);


            Assert.Equal(expectedPassing, SM2Helper.IsPassingQuality(quality));
        }


        [Fact]
        public void NextIntervalAndEf_WhenQualityLow_ShouldResetInterval()
        {
            (int nextInterval, double nextEf) = SM2Helper.NextIntervalAndEf(2.5, 6, repetitionCounter: 2, quality: 2.9);


            Assert.Equal(1, nextInterval);
            Assert.True(2.5 > nextEf);
        }

        [Fact]
        public void NextIntervalAndEf_ShouldFollowClassicSM2Formulas()
        {
            (int nextInterval, double nextEf) = SM2Helper.NextIntervalAndEf(2.5, 5, repetitionCounter: 2, quality: 5);


            Assert.Equal(Math.Ceiling(2.5 * 5), nextInterval);
            Assert.True(2.5 < nextEf);
        }
    }
}