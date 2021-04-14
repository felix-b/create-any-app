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
    public class LexicalErrorDetectionTests
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
        public void NoRuleMatch()
        {
            var grammar = new Grammar<char, Token>(
                new Rule<char, Token>[] {
                    new Rule<char, Token>("AP", new IState<char>[] {
                        new CharState('A'),
                        new CharState('P'),
                    }, match => new AToken(match)),
                    new Rule<char, Token>("BQ", new IState<char>[] {
                        new CharState('B'),
                        new CharState('Q'),
                    }, match => new BToken(match)),
                }
            );
            var lexer = new LexicalAnalysis();
            var reader = CreateSourceReader("APXBQ");
            var tokens = lexer.RunToEnd(grammar, reader).ToArray();
            var diagnostics = reader.Diagnostics;

            CollectionAssert.AreEqual(tokens.Select(p => p.Name), new[] { "A" });
            diagnostics.Count.Should().Be(1);
            diagnostics[0].Marker.Value.Should().Be(2);
            diagnostics[0].ToString().Should().Be("Expected AP or BQ, but found: X");
        }

        [Test]
        public void PartialRuleMatch()
        {
            var grammar = new Grammar<char, Token>(
                new Rule<char, Token>[] {
                    new Rule<char, Token>("ABC", new IState<char>[] {
                        new CharState('A'),
                        new CharState('B'),
                        new CharState('C'),
                    }, match => new AToken(match)),
                    new Rule<char, Token>("DEF", new IState<char>[] {
                        new CharState('D'),
                        new CharState('E'),
                        new CharState('F'),
                    }, match => new BToken(match)),
                }
            );
            var lexer = new LexicalAnalysis();
            var reader = CreateSourceReader("ABCDEXFABC");
            var tokens = lexer.RunToEnd(grammar, reader).ToArray();
            var diagnostics = reader.Diagnostics;

            CollectionAssert.AreEqual(tokens.Select(p => p.Name), new[] { "A" });
            diagnostics.Count.Should().Be(1);
            diagnostics[0].Marker.Value.Should().Be(5);
            diagnostics[0].ToString().Should().Be("Unexpected character: 'X', expected 'F'");
        }

        [Test]
        public void PartialNestedRuleMatch()
        {
            var innerRule = new Rule<char, Token>("B", new IState<char>[] {
                new CharState('B'),
            }, match => new AToken(match));
            
            var outerRule = new Rule<char, Token>("ABC", new IState<char>[] {
                new CharState('A'),
                new RuleRefState<char, Token>("refB", innerRule, Quantifier.Once),
                new CharState('C'), 
            }, match => new AToken(match));
            
            var grammar = new Grammar<char, Token>(new[] {
                outerRule
            });
            
            var lexer = new LexicalAnalysis();
            var reader = CreateSourceReader("AxC");
            var tokens = lexer.RunToEnd(grammar, reader).ToArray();
            var diagnostics = reader.Diagnostics;

            tokens.Length.Should().Be(0);
            diagnostics.Count.Should().Be(1);
            diagnostics[0].Marker.Value.Should().Be(1);
            diagnostics[0].ToString().Should().Be("Expected B, but found: 'x'");
        }

        [Test]
        public void PartialChoiceMatch()
        {
            var itemChoice = new Choice<char, Token>(
                new Rule<char, Token>[] {
                    new Rule<char, Token>("ABC", new IState<char>[] {
                        new CharState('A'),
                        new CharState('B'),
                        new CharState('C'),
                    }, match => new AToken(match)),
                    new Rule<char, Token>("DEF", new IState<char>[] {
                        new CharState('D'),
                        new CharState('E'),
                        new CharState('F'),
                    }, match => new BToken(match)),
                }
            );

            var singleItemListRule = new Rule<char, Token>("LST", new IState<char>[] {
                new CharState('{'),
                new ChoiceRefState<char, Token>("ITM", itemChoice, Quantifier.AtLeastOnce),
                new CharState('}'),
            }, match => new CToken(match));

            var grammar = new Grammar<char, Token>(singleItemListRule);

            var lexer = new LexicalAnalysis();
            var reader = CreateSourceReader("{ABCDEXABC}");
            var tokens = lexer.RunToEnd(grammar, reader).ToArray();
            var diagnostics = reader.Diagnostics;

            CollectionAssert.AreEqual(tokens.Select(p => p.Name), new string[] {});
            diagnostics.Count.Should().Be(1);
            diagnostics[0].Marker.Value.Should().Be(6);
            diagnostics[0].ToString().Should().Be("Unexpected character: 'X', expected 'F'");
        }

        [Test]
        public void PartialNestedChoicesMatch()
        {
            var itemChoice = new Choice<char, Token>(
                new Rule<char, Token>[] {
                    new Rule<char, Token>("ABC", new IState<char>[] {
                        new CharState('A'),
                        new CharState('B'),
                        new CharState('C'),
                    }, match => new AToken(match)),
                    new Rule<char, Token>("DEF", new IState<char>[] {
                        new CharState('D'),
                        new CharState('E'),
                        new CharState('F'),
                    }, match => new BToken(match)),
                }
            );

            var itemRule = new Rule<char, Token>("ITM", new IState<char>[] {
                new ChoiceRefState<char, Token>("ITM", itemChoice, Quantifier.Once)
            }, match => match.FindChoiceByStateId("ITM")!.FindSingleRuleProductOrThrow<Token>());

            var nextItemRule = new Rule<char, Token>("LST-ITM-1ST", new IState<char>[] {
                new CharState(','),
                new RuleRefState<char, Token>("ITM", itemRule, Quantifier.Once),
            }, match => match.FindRuleByStateId("ITM")!.FindSingleRuleProductOrThrow<Token>());

            var listChoice = new Choice<char, Token>(new Rule<char, Token>[] {
                new Rule<char, Token>("EMP-LST", new IState<char>[] {
                    new CharState('{'),
                    new CharState('}'),
                }, match => new XToken(match)),
                new Rule<char, Token>("NON-EMP-LST", new IState<char>[] {
                    new CharState('{'),
                    new ChoiceRefState<char, Token>("ITM-1ST", itemChoice, Quantifier.Once),
                    new RuleRefState<char, Token>("ITM-NXT", nextItemRule, Quantifier.Any),
                    new CharState('}'),
                }, match => new YToken(match))
            });

            var listRule = new Rule<char, Token>("LST", new IState<char>[] {
                new ChoiceRefState<char, Token>("LST", listChoice, Quantifier.Once)
            }, match => new ZToken(match));

            var grammar = new Grammar<char, Token>(itemRule, listRule);

            var lexer = new LexicalAnalysis();
            var reader = CreateSourceReader("{ABC,ABCDEF}");
            var tokens = lexer.RunToEnd(grammar, reader).ToArray();
            var diagnostics = reader.Diagnostics;

            CollectionAssert.AreEqual(tokens.Select(p => p.Name), new string[] {});
            diagnostics[0].Marker.Value.Should().Be(8);
            diagnostics[0].ToString().Should().Be("Unexpected character: 'D', expected '}'");
        }

        [Test]
        public void BacktrackingChoicesMatch()
        {
            var grammar = new Grammar<char, Token>(
                new Rule<char, Token>[] {
                    new Rule<char, Token>("AB", new IState<char>[] {
                        new CharState('A'),
                        new CharState('B'),
                    }, match => new AToken(match)),
                    new Rule<char, Token>("ABCD", new IState<char>[] {
                        new CharState('A'),
                        new CharState('B'),
                        new CharState('C'),
                        new CharState('D'),
                    }, match => new BToken(match)),
                }
            );
            var lexer = new LexicalAnalysis();
            var reader = CreateSourceReader("ABCDABCXAB");
            var tokens = lexer.RunToEnd(grammar, reader).ToArray();
            var diagnostics = reader.Diagnostics;

            CollectionAssert.AreEqual(tokens.Select(p => p.Name), new[] { "B", "A" });
            diagnostics.Count.Should().Be(1);
            diagnostics[0].ToString().Should().Be("Unexpected character: 'X', expected 'D'");
            diagnostics[0].Marker.Value.Should().Be(7);
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
