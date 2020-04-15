using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;
using System;
using System.Collections;

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

    public class SyntaxList : SyntaxNode, IEnumerable<SyntaxNode>
    {
        public SyntaxList(IEnumerable<SyntaxNode> children) 
            : base(SourceSpan.Union(children.Select(c => c.Span)), children)
        {
        }

        public IEnumerator<SyntaxNode> GetEnumerator()
        {
            return Children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static readonly SyntaxList Empty = new SyntaxList(Array.Empty<SyntaxNode>());

        public static SyntaxList Construct(RuleMatch<Token, SyntaxNode> match, IInputContext<Token> context)
        {
            var flatNodes = Flatten(match.MatchedStates.SelectMany(GetSyntaxNodesFromState));
            return new SyntaxList(flatNodes);
        }

        public static SyntaxList ConstructOfType<T>(RuleMatch<Token, SyntaxNode> match, IInputContext<Token> context)
            where T : SyntaxNode
        {
            var flatNodes = Flatten(match.MatchedStates.SelectMany(GetSyntaxNodesFromState));
            return new SyntaxList(flatNodes.Where(node => node is T));
        }

        public static IEnumerable<SyntaxNode> Flatten(IEnumerable<SyntaxNode> nodes)
        {
            return nodes.SelectMany(node => {
                if (node is SyntaxList list)
                {
                    return Flatten(list);
                }
                return new[] { node };
            });
        }

        private static IEnumerable<SyntaxNode> GetSyntaxNodesFromState(IStateMatch<Token> state)
        {
            return state switch 
            {
                IRuleRefStateMatch<Token, SyntaxNode> ruleRef =>
                    ruleRef.RuleMatches
                        .Where(m => m.Product.HasValue)
                        .Select(m => m.Product.Value),
                IChoiceRefStateMatch<Token, SyntaxNode> choiceRef => 
                    choiceRef.GrammarMatches
                        .Where(g => g.MatchedRule?.Product.HasValue == true)
                        .Select(g => g.MatchedRule!.Product.Value),
                _ => Enumerable.Repeat(new TokenSyntax(state.Input), 1)
            };
        }
    }
}
