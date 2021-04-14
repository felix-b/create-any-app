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
                quantifier,
                new BacktrackLabelDescription<Token>($"LL003[token={id}]", d => TokenState.FormatFailure(tokenType.Name, d.Input)))
        {
            TokenType = tokenType;
            Negating = negating;
        }

        public Type TokenType { get; }
        public bool Negating { get; }

        private static string FormatFailure(string expectedTokenType, Token? actualInput)
        {
            return actualInput != null 
                ? $"Expected {expectedTokenType}, but found: {actualInput.Span.GetText()}"
                : $"Expected {expectedTokenType}";
        }
    }
}
