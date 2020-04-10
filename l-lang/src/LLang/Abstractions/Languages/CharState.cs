using System;

namespace LLang.Abstractions.Languages
{
    public class CharState : SimpleState<char>
    {
        public CharState(
            char c, 
            Quantifier? quantifier = null) 
            : this($"#{c}", c, negating: false, quantifier)
        {
            Char = c;
        }

        public CharState(
            string id, 
            char c, 
            Quantifier? quantifier = null) 
            : this(id, c, negating: false, quantifier)
        {
            Char = c;
        }

        public CharState(
            string id, 
            char c, 
            bool negating,
            Quantifier? quantifier) 
            : base(
                id, 
                context => negating
                    ? context.Input != c
                    : context.Input == c, 
                quantifier)
        {
            Char = c;
            Negating = negating;
        }

        public char Char { get; }
        public bool Negating { get; }
    }
}