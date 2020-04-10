using System;

namespace LLang.Abstractions.Languages
{
    public class CharClassState : SimpleState<char>
    {
        public CharClassState(
            string id, 
            CharClass @class, 
            bool negating,
            Quantifier? quantifier) 
            : base(
                id, 
                context => negating
                    ? !IsCharOfClass(context.Input, @class)
                    : IsCharOfClass(context.Input, @class), 
                quantifier)
        {
            Class = @class;
            Negating = negating;
        }

        public CharClass Class { get; }
        public bool Negating { get; }

        private static bool IsCharOfClass(char c, CharClass @class)
        {
            return @class switch 
            {
                CharClass.Control => char.IsControl(c),
                CharClass.Whitespace => char.IsWhiteSpace(c),
                CharClass.Digit => char.IsDigit(c),
                CharClass.Letter => char.IsLetter(c),
                CharClass.Upper => char.IsUpper(c),
                CharClass.Lower => char.IsLower(c),
                _ => false
            };
        }
    }
}