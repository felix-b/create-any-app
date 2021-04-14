using System;
using LLang.Abstractions;
using LLang.Abstractions.Languages;
using NUnit.Framework;
using FluentAssertions;
using System.IO;
using System.Linq;
using LLang.Tracing;

namespace LLang.Tests.Abstractions.Languages
{
    [TestFixture]
    public class LexicalErrorRecoveryTests
    {
        private (Token[] tokens, IReadOnlyDiagnosticList<char> diagnostics) RunLexerTest(
            Grammar<char, Token> grammar, 
            string input)
        {
            var lexer = new LexicalAnalysis();
            var reader = new SourceFileReader(new NoopTrace(), "test.src", new StringReader(input));
            var tokens = lexer.RunToEnd(grammar, reader).ToArray();
            var diagnostics = reader.Diagnostics;

            return (tokens, diagnostics);
        }
            
        private Grammar<char, Token> CreateTestGrammar() => new Grammar<char, Token>(new[] {
            new Rule<char, Token>("Aa", new IState<char>[] {
                new CharState('A'),
                new CharState('a'),
            }, match => new AToken(match)),
            new Rule<char, Token>("Bb", new IState<char>[] {
                new CharState('B'),
                new CharState('b'),
            }, match => new BToken(match)),
        });  
        
        [Test, Ignore("WIP")]
        public void EmbedBadInputInLexicalErrorToken()
        {
            var input = "AaXxBbAa";
            var (tokens, diagnostics) = RunLexerTest(CreateTestGrammar(), input);

            CollectionAssert.AreEqual(
                new[] { "A", "LexicalError", "B", "A" },
                tokens.Select(p => p.Name).ToArray()
            );
            
            diagnostics.Count.Should().Be(1);
            diagnostics[0].Marker.Value.Should().Be(2);
            diagnostics[0].ToString().Should().Be("Unexpected character: 'X'");

            tokens[1].Should().BeOfType<LexicalErrorToken>();
            tokens[1].Span.GetText().Should().Be("Xb");
        }

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

        public class CToken : Token
        {
            public CToken(IMatch<char> match) : base("C", match)
            {
            }
        }

        public class XToken : Token
        {
            public XToken(IMatch<char> match) : base("X", match)
            {
            }
        }

        public class YToken : Token
        {
            public YToken(IMatch<char> match) : base("Y", match)
            {
            }
        }

        public class ZToken : Token
        {
            public ZToken(IMatch<char> match) : base("Z", match)
            {
            }
        }

    }
}
