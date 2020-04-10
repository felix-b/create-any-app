namespace LLang.Abstractions
{
    public class Quantifier
    {
        public Quantifier(int? min, int? max)
        {
            Min = min;
            Max = max;
        }
        public bool Allows(int times)
        {
            return !Max.HasValue || times <= Max.Value;
        }
        public bool IsMetBy(int times)
        {
            return (!Min.HasValue || times >= Min.Value) && (!Max.HasValue || times <= Max.Value);
        }
        public int? Min { get; }
        public int? Max { get; }
        public static Quantifier Any { get; } = new Quantifier(null, null);
        public static Quantifier Once { get; } = new Quantifier(1, 1);
        public static Quantifier AtMostOnce { get; } = new Quantifier(0, 1);
        public static Quantifier AtLeastOnce { get; } = new Quantifier(1, null);
        public static Quantifier Range(int min, int max) => new Quantifier(min, max);
        public static Quantifier Exactly(int times) => new Quantifier(times, times);
        public static Quantifier AtLeast(int minTimes) => new Quantifier(minTimes, null);
        public static Quantifier AtMost(int maxTimes) => new Quantifier(null, maxTimes);
    }
}
