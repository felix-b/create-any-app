using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;
using System;

namespace LLang.Abstractions.Languages
{
    public abstract class SyntaxNode
    {
        protected SyntaxNode(SourceSpan span, IEnumerable<SyntaxNode> children)
        {
            Span = span;
            Children = children.ToImmutableList();
        }

        public override string ToString()
        {
            return this.GetType().Name;
        }


        public SourceSpan Span { get; }
        public ImmutableList<SyntaxNode> Children { get; }
    }

    public class TokenSyntax : SyntaxNode
    {
        public TokenSyntax(Token token) 
            : base(token.Span, new SyntaxNode[0])
        {
            Token = token;
        }

        public Token Token { get; }

        public static TokenSyntax Create(RuleMatch<Token, SyntaxNode> match, IInputContext<Token> context)
        {
            if (match.MatchedStates.Count != 1)
            {
                throw new Exception(
                    $"TokenSyntax: invalid matched state count: ${match.MatchedStates.Count}");
            }
            return new TokenSyntax(match.MatchedStates[0].Input);
        }
    }

    public class SyntaxList : SyntaxNode
    {
        public SyntaxList(IEnumerable<SyntaxNode> children) 
            : base(SourceSpan.Union(children.Select(c => c.Span)), children)
        {
        }

        public static SyntaxList Create(RuleMatch<Token, SyntaxNode> match, IInputContext<Token> context)
        {
            return new SyntaxList(match.MatchedStates.Select(state => new TokenSyntax(state.Input)));
        }
    }
}
