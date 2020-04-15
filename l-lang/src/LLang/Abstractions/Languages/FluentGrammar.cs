
using System;
using System.Collections.Generic;
using System.Linq;
using LLang.Utilities;

namespace LLang.Abstractions.Languages
{

    public static class GrammarExtensions
    {
        public static FluentGrammar<TIn, TOut> Build<TIn, TOut>(this Choice<TIn, TOut> choice)
        {
            return new FluentGrammar<TIn, TOut>(choice);
        }
    }

    public static class FluentGrammar
    {
        public static Type? TryGetTargetType(Delegate d)
        {
            return d.Method?.DeclaringType;
        }

        public static string TryGetTargetTypeName(Delegate d)
        {
            return TryGetTargetType(d)?.Name ?? string.Empty;
        }
    }

    public class FluentGrammar<TIn, TOut>
    {
        public FluentGrammar(Choice<TIn, TOut> choice)
        {
            ThisChoice = choice;
        }

        public FluentGrammar<TIn, TOut> Rule(
            out Rule<TIn, TOut> rule,
            Func<RuleMatch<TIn, TOut>, IInputContext<TIn>, TOut> createProduct,
            Action<FluentRule<TIn, TOut>>? build = null)
        {
            return Rule(
                id: FluentGrammar.TryGetTargetTypeName(createProduct), 
                out rule, 
                createProduct, 
                build);
        }

        public FluentGrammar<TIn, TOut> Rule(
            Func<RuleMatch<TIn, TOut>, IInputContext<TIn>, TOut> createProduct,
            Action<FluentRule<TIn, TOut>>? build = null)
        {
            return Rule(
                id: FluentGrammar.TryGetTargetTypeName(createProduct), 
                out _, 
                createProduct, 
                build);
        }

        public FluentGrammar<TIn, TOut> Rule(
            string id, 
            Func<RuleMatch<TIn, TOut>, IInputContext<TIn>, TOut> createProduct,
            Action<FluentRule<TIn, TOut>>? build = null)
        {
            return Rule(id, out _, createProduct, build);
        }

        public FluentGrammar<TIn, TOut> Rule(
            string id, 
            out Rule<TIn, TOut> rule,
            Func<RuleMatch<TIn, TOut>, IInputContext<TIn>, TOut> createProduct,
            Action<FluentRule<TIn, TOut>>? build = null)
        {
            rule = new Rule<TIn, TOut>(id, createProduct);
            build?.Invoke(rule.Build());
            ThisChoice.Rules.Add(rule);
            return this;
        }

        public FluentGrammar<TIn, TOut> Rule(Rule<TIn, TOut> rule)
        {
            ThisChoice.Rules.Add(rule);
            return this;
        }

        public Choice<TIn, TOut> ThisChoice { get; }
    }

    public static class RuleExtensions
    {
        public static FluentRule<TIn, TOut> Build<TIn, TOut>(this Rule<TIn, TOut> rule)
        {
            return new FluentRule<TIn, TOut>(rule);
        }
    }

    public class FluentRule<TIn, TOut>
    {
        public FluentRule(Rule<TIn, TOut> rule)
        {
            ThisRule = rule;
        }

        public FluentRule<TIn, TOut> Group(
            string id,
            Func<RuleMatch<TIn, TOut>, IInputContext<TIn>, TOut> groupProduct, 
            Action<FluentRule<TIn, TOut>> build,
            Quantifier? quantifier = null)
        {
            var subRule = new Rule<TIn, TOut>(id, groupProduct);
            build(subRule.Build());
            ThisRule.States.Add(new RuleRefState<TIn, TOut>(id, subRule, quantifier));
            return this;
        }

        public FluentRule<TIn, TOut> Group(
            Func<RuleMatch<TIn, TOut>, IInputContext<TIn>, TOut> groupProduct, 
            Action<FluentRule<TIn, TOut>> build,
            Quantifier? quantifier = null)
        {
            return Group(string.Empty, groupProduct, build, quantifier);
        }

        public FluentRule<TIn, TOut> Rule(
            Rule<TIn, TOut> rule,
            Quantifier? quantifier = null)
        {
            return Rule(rule.Id, rule, quantifier);
        }

        public FluentRule<TIn, TOut> Rule(
            string id, 
            Rule<TIn, TOut> rule,
            Quantifier? quantifier = null)
        {
            ThisRule.States.Add(new RuleRefState<TIn, TOut>(id, rule, quantifier));
            return this;
        }

        public FluentRule<TIn, TOut> Choice(
            Action<FluentGrammar<TIn, TOut>> build,
            Quantifier? quantifier = null)
        {
            return Choice(id: string.Empty, build, quantifier);
        }

        public FluentRule<TIn, TOut> Choice(
            string id,
            Action<FluentGrammar<TIn, TOut>> build,
            Quantifier? quantifier = null)
        {
            var subGrammar = new Choice<TIn, TOut>();
            build(subGrammar.Build());
            ThisRule.States.Add(new ChoiceRefState<TIn, TOut>(id, subGrammar, quantifier));
            return this;
        }
        public Rule<TIn, TOut> ThisRule { get; } 
    }

    public static class FluentRuleLexicalExtensions
    {
        public static FluentRule<char, Token> AnyChar(this FluentRule<char, Token> rule, Quantifier? quantifier = null)
        {
            rule.ThisRule.States.Add(new AnyCharState(quantifier));
            return rule;
        }

        public static FluentRule<char, Token> Char(this FluentRule<char, Token> rule, char c, Quantifier? quantifier = null)
        {
            rule.ThisRule.States.Add(new CharState($"{c}", c, negating: false, quantifier));
            return rule;
        }

        public static FluentRule<char, Token> NotChar(this FluentRule<char, Token> rule, char c, Quantifier? quantifier = null)
        {
            rule.ThisRule.States.Add(new CharState($"!{c}", c, negating: true, quantifier));
            return rule;
        }

        public static FluentRule<char, Token> Class(this FluentRule<char, Token> rule, CharClass @class, Quantifier? quantifier = null)
        {
            rule.ThisRule.States.Add(new CharClassState($"<{@class}>", @class, negating: false, quantifier));
            return rule;
        }

        public static FluentRule<char, Token> NotClass(this FluentRule<char, Token> rule, CharClass @class, Quantifier? quantifier = null)
        {
            rule.ThisRule.States.Add(new CharClassState($"!<{@class}>", @class, negating: true, quantifier));
            return rule;
        }

        public static FluentRule<char, Token> CharRange(this FluentRule<char, Token> rule, ValueTuple<char, char>[] ranges, Quantifier? quantifier = null)
        {
            var id = GetIdFromCharRanges(ranges);
            rule.ThisRule.States.Add(CharRangeState.Create(id, negating: false, quantifier, ranges));
            return rule;
        }

        public static FluentRule<char, Token> CharRange(this FluentRule<char, Token> rule, ValueTuple<char, char> range, Quantifier? quantifier = null)
        {
            var ranges = new[] { range };
            var id = GetIdFromCharRanges(ranges);
            rule.ThisRule.States.Add(CharRangeState.Create(id, negating: false, quantifier, ranges));
            return rule;
        }

        public static FluentRule<char, Token> CharRange(this FluentRule<char, Token> rule, string chars, Quantifier? quantifier = null)
        {
            var id = GetIdFromCharRanges(chars);
            rule.ThisRule.States.Add(CharRangeState.Create(id, negating: false, quantifier, GetRangesFromString(chars)));
            return rule;
        }

        public static FluentRule<char, Token> NotCharRange(this FluentRule<char, Token> rule, ValueTuple<char, char>[] ranges, Quantifier? quantifier = null)
        {
            var id = GetIdFromCharRanges(ranges, negating: true);
            rule.ThisRule.States.Add(CharRangeState.Create(id, negating: true, quantifier, ranges));
            return rule;
        }

        public static FluentRule<char, Token> NotCharRange(this FluentRule<char, Token> rule, ValueTuple<char, char> range, Quantifier? quantifier = null)
        {
            var ranges = new[] { range };
            var id = GetIdFromCharRanges(ranges, negating: true);
            rule.ThisRule.States.Add(CharRangeState.Create(id, negating: true, quantifier, ranges));
            return rule;
        }

        public static FluentRule<char, Token> NotCharRange(this FluentRule<char, Token> rule, string chars, Quantifier? quantifier = null)
        {
            var id = GetIdFromCharRanges(chars, negating: true);
            rule.ThisRule.States.Add(CharRangeState.Create(id, negating: true, quantifier, GetRangesFromString(chars)));
            return rule;
        }

        public static FluentRule<char, Token> String(this FluentRule<char, Token> rule, string s, Quantifier? quantifier = null)
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

        private static string GetIdFromCharRanges(ValueTuple<char, char>[] ranges, bool negating = false)
        {
            var negationPrefix = negating ? "!" : string.Empty;
            return $"{negationPrefix}[{string.Join(",", ranges.Select(r => $"{r.Item1}-{r.Item2}"))}]";
        }

        private static string GetIdFromCharRanges(string chars, bool negating = false)
        {
            var negationPrefix = negating ? "!" : string.Empty;
            return negationPrefix + string.Join(string.Empty, chars.Select(c => c.EscapeIfControl()));
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
        public static FluentRule<Token, SyntaxNode> Token<TToken>(this FluentRule<Token, SyntaxNode> rule, Quantifier? quantifier = null)
            where TToken : Token
        {
            return Token<TToken>(rule, stateId: typeof(TToken).Name, quantifier);
        }

        public static FluentRule<Token, SyntaxNode> Token<TToken>(this FluentRule<Token, SyntaxNode> rule, string stateId, Quantifier? quantifier = null)
            where TToken : Token
        {
            rule.ThisRule.States.Add(new TokenState(stateId, typeof(TToken), negating: false, quantifier));
            return rule;
        }

        public static FluentRule<Token, SyntaxNode> NotToken<TToken>(this FluentRule<Token, SyntaxNode> rule, Quantifier? quantifier = null)
            where TToken : Token
        {
            return NotToken<TToken>(rule, stateId: typeof(TToken).Name, quantifier);
        }

        public static FluentRule<Token, SyntaxNode> NotToken<TToken>(this FluentRule<Token, SyntaxNode> rule, string stateId, Quantifier? quantifier = null)
            where TToken : Token
        {
            rule.ThisRule.States.Add(new TokenState(stateId, typeof(TToken), negating: true, quantifier));
            return rule;
        }
    }
}
