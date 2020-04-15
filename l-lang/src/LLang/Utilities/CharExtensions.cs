namespace LLang.Utilities
{
    public static class CharExtensions
    {
        public static string EscapeIfControl(this char c)
        {
            return c <= 32
                ? $"'\\x{(int)c:X2}'"
                : $"'{c}'";
        }
    }
}
