using LLang.Abstractions;
using LLang.Abstractions.Languages;
using LLang.Utilities;

namespace LLang.Tests.Abstractions
{
    public static class GrammarRepository
    {
        public static Rule<char, Token> RuleAa() => new Rule<char, Token>("A", new IState<char>[] {
            new CharState('A'),
            new CharState('a'),
        }, m => new AToken(m));

        public static Rule<char, Token> RuleBb() => new Rule<char, Token>("B", new IState<char>[] {
            new CharState('B'),
            new CharState('b'),
        }, m => new BToken(m));

        public static Rule<char, Token> AaBbRecoveryRule() => new Rule<char, Token>("E", new IState<char>[] {
            CharRangeState.Create("err1", negating: true, Quantifier.Any, LexerUtility.CharRangesFromString("AB"))
        }, m => new ErrToken(m));
        
        public class AToken : Token
        {
            public AToken(IMatch<char> match) : base("A", match)
            {
            }
        }

        public class BToken : Token
        {
            public BToken(IMatch<char> match) : base("B", match)
            {
            }
        }

        public class ErrToken : Token
        {
            public ErrToken(IMatch<char> match) : base("ERR", match)
            {
            }
        }
    }
}