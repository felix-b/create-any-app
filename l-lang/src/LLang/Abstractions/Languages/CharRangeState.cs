using System;
using System.Collections.Immutable;

namespace LLang.Abstractions.Languages
{
    public class CharRangeState : SimpleState<char>
    {
        public CharRangeState(
            string id, 
            ImmutableList<(char from, char to)> ranges, 
            bool negating,
            Quantifier? quantifier) 
            : base(
                id, 
                context => negating
                    ? !IsCharInRanges(context.Input, ranges)
                    : IsCharInRanges(context.Input, ranges), 
                quantifier)
        {
            Ranges = ranges;
            Negating = negating;
        }

        public ImmutableList<(char from, char to)> Ranges { get; }
        public bool Negating { get; }

        public static CharRangeState Create(
            string id, 
            bool negating, 
            Quantifier? quantifier, 
            params (char from, char to)[] ranges)
        {
            return new CharRangeState(id, ranges.ToImmutableList(), negating, quantifier);
        }

        private static bool IsCharInRanges(char c, ImmutableList<(char from, char to)> ranges)
        {
            for (int i = 0 ; i < ranges.Count ; i++)
            {
                var (from, to) = ranges[i];
                if (c >= from && c <= to)
                {
                    return true;
                }
            }
            return false;
        }
    }
}