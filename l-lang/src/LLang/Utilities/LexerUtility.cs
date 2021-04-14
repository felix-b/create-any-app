using System;
using System.Linq;

namespace LLang.Utilities
{
    public static class LexerUtility
    {
        public static ValueTuple<char, char>[] CharRangesFromString(string s)
        {
            var sortedChars = s.Distinct().OrderBy(c => c).ToArray();

            ValueTuple<char, char>[] ranges = sortedChars.Length > 0 && AreConsecutiveChars(sortedChars) 
                ? new ValueTuple<char, char>[] { (sortedChars[0], sortedChars[^1]) }
                : s.Select(c => new ValueTuple<char, char>(c, c)).ToArray();

            return ranges;

            static bool AreConsecutiveChars(char[] chars)
            {
                for (int i = 1 ; i < chars.Length ; i++)
                {
                    if (chars[i] != chars[i-1] + 1)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}