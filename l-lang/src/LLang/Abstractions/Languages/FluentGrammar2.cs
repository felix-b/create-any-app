
using System;
using System.Collections.Generic;
using System.Linq;

namespace LLang.Abstractions.Languages
{

    public static class GrammarExtensions
    {
        public static FluentGrammar2<TIn, TOut> Build<TIn, TOut>(this Grammar<TIn, TOut> grammar)
        {
            return new FluentGrammar2<TIn, TOut>(grammar);
        }
    }

    public class FluentGrammar2<TIn, TOut>
    {
        public FluentGrammar2(Grammar<TIn, TOut> grammar)
        {
            ThisGrammar = grammar;
        }

        public FluentGrammar2<TIn, TOut> Rule(
            out Rule<TIn, TOut> rule,
            Func<RuleMatch<TIn, TOut>, IInputContext<TIn>, TOut> createProduct,
            Action<FluentRule2<TIn, TOut>>? build = null)
        {
            return Rule(id: string.Empty, out rule, createProduct, build);
        }

        public FluentGrammar2<TIn, TOut> Rule(
            Func<RuleMatch<TIn, TOut>, IInputContext<TIn>, TOut> createProduct,
            Action<FluentRule2<TIn, TOut>>? build = null)
        {
            return Rule(id: string.Empty, out _, createProduct, build);
        }

        public FluentGrammar2<TIn, TOut> Rule(
            string id, 
            Func<RuleMatch<TIn, TOut>, IInputContext<TIn>, TOut> createProduct,
            Action<FluentRule2<TIn, TOut>>? build = null)
        {
            return Rule(id, out _, createProduct, build);
        }

        public FluentGrammar2<TIn, TOut> Rule(
            string id, 
            out Rule<TIn, TOut> rule,
            Func<RuleMatch<TIn, TOut>, IInputContext<TIn>, TOut> createProduct,
            Action<FluentRule2<TIn, TOut>>? build = null)
        {
            rule = new Rule<TIn, TOut>(id, createProduct);
            build?.Invoke(rule.Build());
            ThisGrammar.Rules.Add(rule);
            return this;
        }

        public FluentGrammar2<TIn, TOut> Rule(Rule<TIn, TOut> rule)
        {
            ThisGrammar.Rules.Add(rule);
            return this;
        }

        public Grammar<TIn, TOut> ThisGrammar { get; }
    }

    public static class RuleExtensions
    {
        public static FluentRule2<TIn, TOut> Build<TIn, TOut>(this Rule<TIn, TOut> rule)
        {
            return new FluentRule2<TIn, TOut>(rule);
        }
    }

    public class FluentRule2<TIn, TOut>
    {
        public FluentRule2(Rule<TIn, TOut> rule)
        {
            ThisRule = rule;
        }

        public FluentRule2<TIn, TOut> Group(
            string id,
            Func<RuleMatch<TIn, TOut>, IInputContext<TIn>, TOut> groupProduct, 
            Action<FluentRule2<TIn, TOut>> build,
            Quantifier? quantifier = null)
        {
            var subRule = new Rule<TIn, TOut>(id, groupProduct);
            build(subRule.Build());
            ThisRule.States.Add(new RuleRefState<TIn, TOut>(id, subRule, quantifier));
            return this;
        }

        public FluentRule2<TIn, TOut> Group(
            Func<RuleMatch<TIn, TOut>, IInputContext<TIn>, TOut> groupProduct, 
            Action<FluentRule2<TIn, TOut>> build,
            Quantifier? quantifier = null)
        {
            return Group(string.Empty, groupProduct, build, quantifier);
        }

        public FluentRule2<TIn, TOut> Rule(
            Rule<TIn, TOut> rule,
            Quantifier? quantifier = null)
        {
            return Rule(rule.Id, rule, quantifier);
        }

        public FluentRule2<TIn, TOut> Rule(
            string id, 
            Rule<TIn, TOut> rule,
            Quantifier? quantifier = null)
        {
            ThisRule.States.Add(new RuleRefState<TIn, TOut>(id, rule, quantifier));
            return this;
        }

        public FluentRule2<TIn, TOut> Choice(
            Action<FluentGrammar2<TIn, TOut>> build,
            Quantifier? quantifier = null)
        {
            return Choice(id: string.Empty, build, quantifier);
        }

        public FluentRule2<TIn, TOut> Choice(
            string id,
            Action<FluentGrammar2<TIn, TOut>> build,
            Quantifier? quantifier = null)
        {
            var subGrammar = new Grammar<TIn, TOut>();
            build(subGrammar.Build());
            ThisRule.States.Add(new GrammarRefState<TIn, TOut>(id, subGrammar, quantifier));
            return this;
        }
        public Rule<TIn, TOut> ThisRule { get; } 
    }

    public static class FluentRuleLexicalExtensions
    {
        public static FluentRule2<char, Token> AnyChar(this FluentRule2<char, Token> rule, Quantifier? quantifier = null)
        {
            rule.ThisRule.States.Add(new AnyCharState(quantifier));
            return rule;
        }

        public static FluentRule2<char, Token> Char(this FluentRule2<char, Token> rule, char c, Quantifier? quantifier = null)
        {
            rule.ThisRule.States.Add(new CharState($"c={c}", c, negating: false, quantifier));
            return rule;
        }

        // public static Rule<char, Token> Char(this Rule<char, Token> rule, Func<char, bool> predicate, Quantifier? quantifier = null)
        // {
        //     rule.States.Add(new SimpleState<char>("cF", context => predicate(context.Input), quantifier));
        //     return rule;
        // }

        public static FluentRule2<char, Token> NotChar(this FluentRule2<char, Token> rule, char c, Quantifier? quantifier = null)
        {
            rule.ThisRule.States.Add(new CharState($"c!={c}", c, negating: true, quantifier));
            return rule;
        }

        // public static Rule<char, Token> NotChar(this Rule<char, Token> rule, Func<char, bool> predicate, Quantifier? quantifier = null)
        // {
        //     rule.States.Add(new SimpleState<char>("cF", context => !predicate(context.Input), quantifier));
        //     return rule;
        // }

        public static FluentRule2<char, Token> Class(this FluentRule2<char, Token> rule, CharClass @class, Quantifier? quantifier = null)
        {
            rule.ThisRule.States.Add(new CharClassState($"cls={@class}", @class, negating: false, quantifier));
            return rule;
        }

        public static FluentRule2<char, Token> NotClass(this FluentRule2<char, Token> rule, CharClass @class, Quantifier? quantifier = null)
        {
            rule.ThisRule.States.Add(new CharClassState($"cls!={@class}", @class, negating: true, quantifier));
            return rule;
        }

        public static FluentRule2<char, Token> CharRange(this FluentRule2<char, Token> rule, ValueTuple<char, char>[] ranges, Quantifier? quantifier = null)
        {
            rule.ThisRule.States.Add(CharRangeState.Create("crng", negating: false, quantifier, ranges));
            return rule;
        }

        public static FluentRule2<char, Token> CharRange(this FluentRule2<char, Token> rule, ValueTuple<char, char> range, Quantifier? quantifier = null)
        {
            rule.ThisRule.States.Add(CharRangeState.Create("crng", negating: false, quantifier, new[] { range }));
            return rule;
        }

        public static FluentRule2<char, Token> CharRange(this FluentRule2<char, Token> rule, string chars, Quantifier? quantifier = null)
        {
            rule.ThisRule.States.Add(CharRangeState.Create("crng", negating: false, quantifier, GetRangesFromString(chars)));
            return rule;
        }

        public static FluentRule2<char, Token> NotCharRange(this FluentRule2<char, Token> rule, ValueTuple<char, char>[] ranges, Quantifier? quantifier = null)
        {
            rule.ThisRule.States.Add(CharRangeState.Create("!crng", negating: true, quantifier, ranges));
            return rule;
        }

        public static FluentRule2<char, Token> NotCharRange(this FluentRule2<char, Token> rule, ValueTuple<char, char> range, Quantifier? quantifier = null)
        {
            rule.ThisRule.States.Add(CharRangeState.Create("!crng", negating: true, quantifier, new[] { range }));
            return rule;
        }

        public static FluentRule2<char, Token> NotCharRange(this FluentRule2<char, Token> rule, string chars, Quantifier? quantifier = null)
        {
            rule.ThisRule.States.Add(CharRangeState.Create("!crng", negating: true, quantifier, GetRangesFromString(chars)));
            return rule;
        }

        public static FluentRule2<char, Token> String(this FluentRule2<char, Token> rule, string s, Quantifier? quantifier = null)
        {
            rule.Group(
                id: s, 
                groupProduct: (m, x) => new Token(s, m, x),
                build: r => {
                    for (int i = 0 ; i < s.Length ; i++)
                    {
                        var c = s[i];
                        r.Char(c);
                    }
                }, 
                quantifier);
            
            return rule;
        }

        private static ValueTuple<char, char>[] GetRangesFromString(string s)
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
    public static class FluentRuleSyntaxExtensions
    {
        public static FluentRule2<Token, SyntaxNode> Token<TToken>(this FluentRule2<Token, SyntaxNode> rule, Quantifier? quantifier = null)
            where TToken : Token
        {
            return Token<TToken>(rule, stateId: typeof(TToken).Name, quantifier);
        }

        public static FluentRule2<Token, SyntaxNode> Token<TToken>(this FluentRule2<Token, SyntaxNode> rule, string stateId, Quantifier? quantifier = null)
            where TToken : Token
        {
            rule.ThisRule.States.Add(new TokenState(stateId, typeof(TToken), negating: false, quantifier));
            return rule;
        }

        public static FluentRule2<Token, SyntaxNode> NotToken<TToken>(this FluentRule2<Token, SyntaxNode> rule, Quantifier? quantifier = null)
            where TToken : Token
        {
            return NotToken<TToken>(rule, stateId: typeof(TToken).Name, quantifier);
        }

        public static FluentRule2<Token, SyntaxNode> NotToken<TToken>(this FluentRule2<Token, SyntaxNode> rule, string stateId, Quantifier? quantifier = null)
            where TToken : Token
        {
            rule.ThisRule.States.Add(new TokenState(stateId, typeof(TToken), negating: true, quantifier));
            return rule;
        }
    }

}
