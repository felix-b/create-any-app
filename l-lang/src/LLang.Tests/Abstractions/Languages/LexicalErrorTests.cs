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
    public class LexicalErrorTests
    {

        // public static readonly ErrorTestCase[] ErrorTestCases = new[] {
        //     new ErrorTestCase {
        //         Input = "",
        //         Success = true
        //     },
        //     new ErrorTestCase {
        //         Input = "",
        //         Success = false,
        //         Assert = (diagnostic) => {
        //             diagnostic.ToString
        //         }
        //     },
        //     new ErrorTestCase {
        //         Input = ""
        //     },
        // };

        // [TestCaseSource(nameof(ErrorTestCases))]
        // public void CanReportErrors(ErrorTestCase testCase)
        // {
        //     var grammar = new Grammar<char, Token>(
        //         new Rule<char, Token>[] {
        //             new Rule<char, Token>("A", new IState<char>[] {
        //                 new CharState('A'),
        //             }, match => new AToken(match)),
        //             new Rule<char, Token>("B", new IState<char>[] {
        //                 new CharState('B'),
        //             }, match => new BToken(match)),
        //         }
        //     );
        //     var lexer = new LexicalAnalysis();
        //     var products = lexer.RunToEnd(grammar, CreateSourceReader(testCase.Input!)).ToArray();

        //     products.Should().NotBeNull();

        //     if (testCase.Success)
        //     {
        //         products.Single().Should().BeOfType<AToken>();
        //     }
        //     else
        //     {
        //         products.Should().BeEmpty();
        //     }
        // }

        [Test]
        public void CanReportErrors()
        {
            var grammar = new Grammar<char, Token>(
                new Rule<char, Token>[] {
                    new Rule<char, Token>("A", new IState<char>[] {
                        new CharState('A'),
                    }, match => new AToken(match)),
                    new Rule<char, Token>("B", new IState<char>[] {
                        new CharState('B'),
                    }, match => new BToken(match)),
                }
            );
            var lexer = new LexicalAnalysis();
            var reader = CreateSourceReader("AXB");
            var tokens = lexer.RunToEnd(grammar, reader).ToArray();
            var diagnostics = reader.Diagnostics;

            CollectionAssert.AreEqual(tokens.Select(p => p.Name), new[] { "A" });
            diagnostics.Count.Should().Be(1);
            diagnostics[0].ToString().Should().Be("Unexpected character: 'X'");
        }

        private SourceFileReader CreateSourceReader(string sourceText)
        {
            return new SourceFileReader(new NoopTrace(), "test.src", new StringReader(sourceText));
        }

        public class ErrorTestCase
        {
            public string? Input { get; set; }
            public bool Success { get; set; }
            public Action<LexicalDiagnostic>? Assert { get; set; }
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
