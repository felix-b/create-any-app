using System;

namespace LLang.Abstractions.Languages
{
    public class TokenState : SimpleState<Token>
    {
        public TokenState(
            string id, 
            Type tokenType, 
            bool negating,
            Quantifier? quantifier) 
            : base(
                id, 
                context => negating
                    ? !tokenType.IsInstanceOfType(context.Input)
                    : tokenType.IsInstanceOfType(context.Input),
                quantifier)
        {
            TokenType = tokenType;
            Negating = negating;
        }

        public Type TokenType { get; }
        public bool Negating { get; }
    }
}