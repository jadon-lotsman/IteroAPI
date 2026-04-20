namespace Mnemo.Common
{
    public static class SM2Helper
    {
        public const double MinEF = 1.3;
        public const double InitEF = 2.5;

        public const int MinInterval = 1;
        public const int MaxInterval = 365;

        private const int FirstIntervalDays = 1;
        private const int SecondIntervalDays = 3;


        public static double ComputeQuality(TimeSpan averageTime, TimeSpan actionTime, int actionCounter, double similarity)
        {
            var changeCounter = Math.Max(0, actionCounter-1);
            double Stability = Math.Exp(-changeCounter);

            double Accuracy = CalcFuzzyAccuracy(similarity);

            var ratio = averageTime / actionTime;
            double Reaction = CalcSigmoidReaction(ratio);

            double Knowledge = 0.5 * Accuracy + 0.3 * Stability + 0.2 * Reaction;

            double Quality = Knowledge * 5;

            return Quality;
        }


        public static (int newInterval, double newEasinessFactor) NextIntervalAndEf(double easinessFactor, int interval, int repetitionCounter, double quality)
        {
            int newInterval;
            double newEasinessFactor;

            newEasinessFactor = easinessFactor + (0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02));
            newEasinessFactor = Math.Max(newEasinessFactor, MinEF);

            if (!IsPassingQuality(quality))
            {
                newInterval = FirstIntervalDays;
            }
            else
            {
                newInterval = repetitionCounter switch
                {
                    0 => FirstIntervalDays,
                    1 => SecondIntervalDays,
                    _ => (int)Math.Ceiling((interval > 0 ? interval : 1) * newEasinessFactor)
                };

                newInterval = Math.Clamp(newInterval, MinInterval, MaxInterval);
            }

            return (newInterval, newEasinessFactor);
        }


        public static bool IsPassingQuality(double quality) => quality >= 3;

        private static double CalcFuzzyAccuracy(double similarity, double min=0.75, double max=0.9)
        {
            if (similarity <= min)  return 0;
            if (similarity >= max) return 1;
            return (similarity - min) / (max - min);
        }

        private static double CalcSigmoidReaction(double ratio, double min=0.7, double max=1.3, double center=1.0, double steepness=3.0)
        {
            return min + (max - min) / (1.0 + Math.Exp(-steepness * (ratio - center)));
        }
    }
}
