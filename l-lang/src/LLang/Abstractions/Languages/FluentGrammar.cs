// using System;
// using System.Collections.Generic;
// using System.Linq;

// namespace LLang.Abstractions.Languages
// {
//     public static class FluentLanguage
//     {
//         public static FluentGrammar<char, Token> NewLexicon() => new FluentGrammar<char, Token>();
//         public static FluentGrammar<Token, SyntaxNode> NewSyntax() => new FluentGrammar<Token, SyntaxNode>();
//     }

//     public class FluentGrammar<TIn, TOut>
//     {
//         private readonly List<FluentRule<TIn, TOut>> _rules = new List<FluentRule<TIn, TOut>>();

//         public FluentGrammar<TIn, TOut> Rule(
//             string id, 
//             Func<FluentRule<TIn, TOut>, FluentRule<TIn, TOut>> build, 
//             Func<RuleMatch<TIn, TOut>, TOut> product)
//         {
//             var rule = new FluentRule<TIn, TOut>(id, new DelegatingProductFactory<TIn, TOut>(product));
//             _rules.Add(rule);
//             build(rule);
//             return this;
//         }

//         public FluentGrammar<TIn, TOut> Rule(
//             string id, 
//             Func<FluentRule<TIn, TOut>, FluentRule<TIn, TOut>> build, 
//             Func<RuleMatch<TIn, TOut>, IInputContext<TIn>, TOut> product)
//         {
//             var rule = new FluentRule<TIn, TOut>(id, new DelegatingProductFactory<TIn, TOut>(product));
//             _rules.Add(rule);
//             build(rule);
//             return this;
//         }

//         public FluentGrammar<TIn, TOut> Rule(
//             out FluentRuleRef<TIn, TOut> ruleRef,
//             string id, 
//             Func<FluentRule<TIn, TOut>, FluentRule<TIn, TOut>> build, 
//             Func<RuleMatch<TIn, TOut>, TOut> product)
//         {
//             var rule = new FluentRule<TIn, TOut>(id, new DelegatingProductFactory<TIn, TOut>(product));
//             _rules.Add(rule);
//             build(rule);
//             ruleRef = new FluentRuleRef<TIn, TOut>(() => rule);
//             return this;
//         }

//         public FluentGrammar<TIn, TOut> Rule(
//             out FluentRuleRef<TIn, TOut> ruleRef,
//             string id, 
//             Func<FluentRule<TIn, TOut>, FluentRule<TIn, TOut>> build, 
//             Func<RuleMatch<TIn, TOut>, IInputContext<TIn>, TOut> product)
//         {
//             var rule = new FluentRule<TIn, TOut>(id, new DelegatingProductFactory<TIn, TOut>(product));
//             _rules.Add(rule);
//             build(rule);
//             ruleRef = new FluentRuleRef<TIn, TOut>(() => rule);
//             return this;
//         }

//         public FluentGrammar<TIn, TOut> Rule<TProduct>(
//             string id, 
//             Func<FluentRule<TIn, TOut>, FluentRule<TIn, TOut>> build)
//             where TProduct : TOut, IProductOfFactory
//         {
//             throw new NotImplementedException();
//         }

//         public FluentGrammar<TIn, TOut> Rule<TProduct>(
//             out FluentRuleRef<TIn, TOut> ruleRef,
//             string id, 
//             Func<FluentRule<TIn, TOut>, FluentRule<TIn, TOut>> build)
//             where TProduct : TOut, IProductOfFactory
//         {
//             throw new NotImplementedException();
//         }

//         public FluentGrammar<TIn, TOut> Rule(FluentRuleRef<TIn, TOut> ruleRef, Quantifier? quantifier = null)
//         {
//             var refRuleId = ruleRef.GetFluent().Id;
//             return Rule(
//                 $"r#{refRuleId}", 
//                 r => r.Group(
//                     $"rg#{refRuleId}", 
//                     ruleRef, 
//                     quantifier
//                 ), 
//                 (m, x) => m.Product.Value
//             );
//         }

//         public Grammar<TIn, TOut> GetGrammar()
//         {
//             return new Grammar<TIn, TOut>(_rules.Select(r => (Rule<TIn, TOut>)r));
//         }
//     }

//     public class FluentRule<TIn, TOut>
//     {
//         private readonly string _id;
//         private readonly IProductFactory<TIn, TOut> _productFactory;
//         private readonly List<IState<TIn>> _states = new List<IState<TIn>>();

//         public FluentRule(string id, DelegatingProductFactory<TIn, TOut> productFactory)
//         {
//             _id = id;
//             _productFactory = productFactory;
//         }

//         public FluentRule<TIn, TOut> Group(
//             string id, 
//             Func<FluentRule<TIn, TOut>, FluentRule<TIn, TOut>> build, 
//             Func<RuleMatch<TIn, TOut>, IInputContext<TIn>, TOut> groupProduct,
//             Quantifier? quantifier = null)
//         {
//             var nestedRule = new FluentRule<TIn, TOut>(id, groupProduct);
//             build(nestedRule);
//             _states.Add(new RuleRefState<TIn, TOut>(id, nestedRule, quantifier));
//             return this;
//         }

//         public FluentRule<TIn, TOut> Group(string id, FluentRuleRef<TIn, TOut> ruleRef, Quantifier? quantifier = null)
//         {
//             _states.Add(new RuleRefState<TIn, TOut>(id, ruleRef.GetRule(), quantifier));
//             return this;
//         }

//         public FluentRule<TIn, TOut> Choice(string id, Func<FluentGrammar<TIn, TOut>, FluentGrammar<TIn, TOut>> build, Quantifier? quantifier = null)
//         {
//             var nestedGrammar = new FluentGrammar<TIn, TOut>();
//             build(nestedGrammar);
//             _states.Add(new GrammarRefState<TIn, TOut>(id, nestedGrammar.GetGrammar(), quantifier));
//             return this;
//         }

//         public FluentRule<TIn, TOut> RawState(IState<TIn> state)
//         {
//             _states.Add(state);
//             return this;
//         }

//         public string Id => _id;

//         private Rule<TIn, TOut> GetRule()
//         {
//             return new Rule<TIn, TOut>(_id, _states, _productFactory);
//         }

//         public static implicit operator Rule<TIn, TOut>(FluentRule<TIn, TOut> fluent)
//         {
//             return fluent.GetRule();
//         }
//     }

//     public static class LexiconFluentRuleExtensions
//     {
//         public static FluentRule<char, Token> AnyChar(this FluentRule<char, Token> rule, Quantifier? quantifier = null)
//         {
//             rule.RawState(new AnyCharState(quantifier));
//             return rule;
//         }

//         public static FluentRule<char, Token> Char(this FluentRule<char, Token> rule, char c, Quantifier? quantifier = null)
//         {
//             rule.RawState(new CharState($"c={c}", c, negating: false, quantifier));
//             return rule;
//         }

//         // public static FluentRule<char, Token> Char(this FluentRule<char, Token> rule, Func<char, bool> predicate, Quantifier? quantifier = null)
//         // {
//         //     rule.RawState(new SimpleState<char>("cF", context => predicate(context.Input), quantifier));
//         //     return rule;
//         // }

//         public static FluentRule<char, Token> NotChar(this FluentRule<char, Token> rule, char c, Quantifier? quantifier = null)
//         {
//             rule.RawState(new CharState($"c!={c}", c, negating: true, quantifier));
//             return rule;
//         }

//         // public static FluentRule<char, Token> NotChar(this FluentRule<char, Token> rule, Func<char, bool> predicate, Quantifier? quantifier = null)
//         // {
//         //     rule.RawState(new SimpleState<char>("cF", context => !predicate(context.Input), quantifier));
//         //     return rule;
//         // }

//         public static FluentRule<char, Token> Class(this FluentRule<char, Token> rule, CharClass @class, Quantifier? quantifier = null)
//         {
//             rule.RawState(new CharClassState($"cls={@class}", @class, negating: false, quantifier));
//             return rule;
//         }

//         public static FluentRule<char, Token> NotClass(this FluentRule<char, Token> rule, CharClass @class, Quantifier? quantifier = null)
//         {
//             rule.RawState(new CharClassState($"cls!={@class}", @class, negating: true, quantifier));
//             return rule;
//         }

//         public static FluentRule<char, Token> CharRange(this FluentRule<char, Token> rule, ValueTuple<char, char>[] ranges, Quantifier? quantifier = null)
//         {
//             rule.RawState(CharRangeState.Create("crng", negating: false, quantifier, ranges));
//             return rule;
//         }

//         public static FluentRule<char, Token> CharRange(this FluentRule<char, Token> rule, ValueTuple<char, char> range, Quantifier? quantifier = null)
//         {
//             rule.RawState(CharRangeState.Create("crng", negating: false, quantifier, new[] { range }));
//             return rule;
//         }

//         public static FluentRule<char, Token> CharRange(this FluentRule<char, Token> rule, string chars, Quantifier? quantifier = null)
//         {
//             rule.RawState(CharRangeState.Create("crng", negating: false, quantifier, GetRangesFromString(chars)));
//             return rule;
//         }

//         public static FluentRule<char, Token> NotCharRange(this FluentRule<char, Token> rule, ValueTuple<char, char>[] ranges, Quantifier? quantifier = null)
//         {
//             rule.RawState(CharRangeState.Create("!crng", negating: true, quantifier, ranges));
//             return rule;
//         }

//         public static FluentRule<char, Token> NotCharRange(this FluentRule<char, Token> rule, ValueTuple<char, char> range, Quantifier? quantifier = null)
//         {
//             rule.RawState(CharRangeState.Create("!crng", negating: true, quantifier, new[] { range }));
//             return rule;
//         }

//         public static FluentRule<char, Token> NotCharRange(this FluentRule<char, Token> rule, string chars, Quantifier? quantifier = null)
//         {
//             rule.RawState(CharRangeState.Create("!crng", negating: true, quantifier, GetRangesFromString(chars)));
//             return rule;
//         }

//         public static FluentRule<char, Token> String(this FluentRule<char, Token> rule, string s, Quantifier? quantifier = null)
//         {
//             rule.Group(
//                 id: s, 
//                 build: r => {
//                     for (int i = 0 ; i < s.Length ; i++)
//                     {
//                         var c = s[i];
//                         r.Char(c);
//                     }
//                     return r;                
//                 }, 
//                 groupProduct: (m, x) => new Token(s, m, x), 
//                 quantifier);
            
//             return rule;
//         }

//         private static ValueTuple<char, char>[] GetRangesFromString(string s)
//         {
//             var sortedChars = s.Distinct().OrderBy(c => c).ToArray();

//             ValueTuple<char, char>[] ranges = sortedChars.Length > 0 && AreConsecutiveChars(sortedChars) 
//                 ? new ValueTuple<char, char>[] { (sortedChars[0], sortedChars[^1]) }
//                 : s.Select(c => new ValueTuple<char, char>(c, c)).ToArray();

//             return ranges;

//             static bool AreConsecutiveChars(char[] chars)
//             {
//                 for (int i = 1 ; i < chars.Length ; i++)
//                 {
//                     if (chars[i] != chars[i-1] + 1)
//                     {
//                         return false;
//                     }
//                 }
//                 return true;
//             }
//         }
//     }

//     public static class SyntaxFluentRuleExtensions
//     {
//         public static FluentRule<Token, SyntaxNode> Token<TToken>(this FluentRule<Token, SyntaxNode> rule, string id, Quantifier? quantifier = null)
//             where TToken : Token
//         {
//             rule.RawState(new TokenState(id, typeof(TToken), negating: false, quantifier));
//             return rule;
//         }

//         public static FluentRule<Token, SyntaxNode> NotToken<TToken>(this FluentRule<Token, SyntaxNode> rule, string id, Quantifier? quantifier = null)
//             where TToken : Token
//         {
//             rule.RawState(new TokenState(id, typeof(TToken), negating: true, quantifier));
//             return rule;
//         }
//     }

//     public class FluentRuleRef<TIn, TOut>
//     {
//         public readonly Func<FluentRule<TIn, TOut>> GetFluent;
//         public FluentRuleRef(Func<FluentRule<TIn, TOut>> getFluent)
//         {
//             GetFluent = getFluent;
//         }
//         public Rule<TIn, TOut> GetRule()
//         {
//             // invokes implicit convertion operator
//             return GetFluent();
//         }
//     }

//     public enum CharClass
//     {
//         Control,
//         Whitespace,
//         Digit,
//         Letter,
//         Lower,
//         Upper
//     }
// }
