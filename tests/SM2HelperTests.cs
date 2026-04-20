using Mnemo.Common;

namespace tests
{
    public class SM2HelperTests
    {
        [Theory]
        [InlineData("apple", "apple", true)]
        [InlineData("apple", "appel", true)]
        [InlineData("apple", "apfel", false)]
        [InlineData("apple", "angel", false)]
        public void PassingTest(string a, string b, bool isPassing)
        {
            TimeSpan average = TimeSpan.FromSeconds(10);
            TimeSpan action = TimeSpan.FromSeconds(12);
            int actionCounter = 1;
            double similarity = a.ComputeLevenshteinSimilarity(b);


            double quality = SM2Helper.ComputeQuality(average, action, actionCounter, similarity);


            Assert.True(SM2Helper.IsPassingQuality(quality) == isPassing);
        }

        [Fact]
        public void IntervalTest()
        {
            (_, double ef5) = SM2Helper.NextIntervalAndEf(SM2Helper.InitEF, 3, 2, 5);
            (_, double ef4) = SM2Helper.NextIntervalAndEf(SM2Helper.InitEF, 3, 2, 4);
            (_, double ef3) = SM2Helper.NextIntervalAndEf(SM2Helper.InitEF, 3, 2, 3);
            (_, double ef2) = SM2Helper.NextIntervalAndEf(SM2Helper.InitEF, 3, 2, 2);
            (_, double ef1) = SM2Helper.NextIntervalAndEf(SM2Helper.InitEF, 3, 2, 1);


            Assert.True(ef5 > ef4);
            Assert.True(ef4 > ef3);
            Assert.True(ef3 > ef2);
            Assert.True(ef2 >= ef1);
        }
    }
}