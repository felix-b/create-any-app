namespace LLang.Abstractions.Languages
{
    public class Token
    {
        public Token(string name, IMatch<char> match, IInputContext<char>? context = null)
        {
            Name = name;
            Span = new SourceSpan(match, context);
        }

        public override string ToString()
        {
            return this.GetType().Name;
        }

        public string Name { get; }
        public SourceSpan Span { get; }

        public static Token CreateToken(IMatch<char> match, IInputContext<char> context)
        {
            return new Token(name: string.Empty, match, context);
        }
    }
}
