using System;
using LLang.Abstractions;
using LLang.Abstractions.Languages;
using NUnit.Framework;
using FluentAssertions;
using System.IO;
using System.Linq;
using LLang.Utilities;

namespace LLang.Tests.Abstractions.Languages
{
    [TestFixture]
    public class LexicalAnalysisTests
    {
        [TestCase("A", true)]
        [TestCase("", false)]
        [TestCase("B", false)]
        public void ParseSingleState(string inputText, bool shouldSucceed)
        {
            var grammar = new Grammar<char, Token>(
                new Rule<char, Token>[] {
                    new Rule<char, Token>("ruleA", new IState<char>[] {
                        new CharState('A')
                    }, match => new AToken(match))
                }
            );
            var lexer = new LexicalAnalysis();

            var products = lexer.RunToEnd(grammar, CreateSourceReader(inputText)).ToArray();

            products.Should().NotBeNull();

            if (shouldSucceed)
            {
                products.Single().Should().BeOfType<AToken>();
            }
            else
            {
                products.Should().BeEmpty();
            }
        }

        [Test]
        public void ParseSingleStateMultipleTimes()
        {
            var grammar = new Grammar<char, Token>(
                new Rule<char, Token>[] {
                    new Rule<char, Token>("ruleA", new IState<char>[] {
                        new CharState('A')
                    }, match => new AToken(match))
                }
            );
            var lexer = new LexicalAnalysis();

            var products = lexer.RunToEnd(grammar, CreateSourceReader("AAA")).ToArray();

            products.Should().NotBeNull();
            products.Length.Should().Be(3);
            products.Should().AllBeOfType<AToken>();
        }

        [Test]
        public void ParseMultipleStates()
        {
            var grammar = new Grammar<char, Token>(
                new Rule<char, Token>[] {
                    new Rule<char, Token>("ruleABC", new IState<char>[] {
                        new CharState('A'),
                        new CharState('B'),
                        new CharState('C')
                    }, match => new AToken(match))
                }
            );
            var lexer = new LexicalAnalysis();

            var products = lexer.RunToEnd(grammar, CreateSourceReader("ABC")).ToArray();

            products.Should().NotBeNull();
            products.Single().Should().BeOfType<AToken>();
        }

        [TestCase("", new string[0])]
        [TestCase("A", new[] { "A" } )]
        [TestCase("B", new[] { "B" } )]
        [TestCase("ABBA", new[] { "A", "B", "B", "A" } )]
        [TestCase("BAAB", new[] { "B", "A", "A", "B" } )]
        public void ParseMultipleRules(string inputText, string[] expectedTokens)
        {
            var grammar = new Grammar<char, Token>(
                new Rule<char, Token>[] {
                    new Rule<char, Token>(
                        "ruleA", 
                        new IState<char>[] {
                            new CharState('A')
                        }, 
                        match => new AToken(match)
                    ),
                    new Rule<char, Token>(
                        "ruleB", 
                        new IState<char>[] {
                            new CharState('B'),
                        }, 
                        match => new BToken(match)
                    )
                }
            );
            
            var lexer = new LexicalAnalysis();
            var tokens = lexer.RunToEnd(grammar, CreateSourceReader(inputText)).ToArray();

            tokens.Should().NotBeNull();
            CollectionAssert.AreEqual(expectedTokens, tokens.Select(t => t.Name));
        }

        [TestCase("", new string[0])]
        [TestCase("x", new string[0])]
        [TestCase("A", new string[0])]
        [TestCase("BB", new string[0])]
        [TestCase("AA", new[] { "A" } )]
        [TestCase("BBB", new[] { "B" } )]
        [TestCase("AABBB", new[] { "A", "B" } )]
        [TestCase("AABBBAABBB", new[] { "A", "B", "A", "B" } )]
        [TestCase("AABBBBBBAA", new[] { "A", "B", "B", "A" } )]
        [TestCase("AABBBxAABBB", new[] { "A", "B" } )]
        public void ParseMultipleRulesWithMultipleStates(string inputText, string[] expectedTokens)
        {
            var grammar = new Grammar<char, Token>(
                new Rule<char, Token>[] {
                    new Rule<char, Token>(
                        "ruleA", 
                        new IState<char>[] {
                            new CharState(id: "A1", 'A'),
                            new CharState(id: "A2", 'A')
                        }, 
                        match => new AToken(match)
                    ),
                    new Rule<char, Token>(
                        "ruleB", 
                        new IState<char>[] {
                            new CharState(id: "B1", 'B'),
                            new CharState(id: "B2", 'B'),
                            new CharState(id: "B3", 'B'),
                        }, 
                        match => new BToken(match)
                    )
                }
            );
            
            var lexer = new LexicalAnalysis();
            var tokens = lexer.RunToEnd(grammar, CreateSourceReader(inputText)).ToArray();

            tokens.Should().NotBeNull();
            CollectionAssert.AreEqual(expectedTokens, tokens.Select(t => t.Name));
        }

        [TestCase("AQQC", new[] { "A" } )]
        [TestCase("AQQQC", new[] { "A" } )]
        [TestCase("AQC", new string[0])]
        [TestCase("AQQQQC", new string[0])]
        [TestCase("AC", new string[0])]
        [TestCase("A", new string[0])]
        [TestCase("C", new string[0])]
        [TestCase("", new string[0] )]
        public void ParseStateWithQuantifierBasic(string inputText, string[] expectedTokens)
        {
            var grammar = new Grammar<char, Token>(
                new Rule<char, Token>[] {
                    new Rule<char, Token>(
                        "ruleA", 
                        new IState<char>[] {
                            new CharState('A'),
                            new CharState('Q', Quantifier.Range(2, 3)),
                            new CharState('C'),
                        }, 
                        match => new AToken(match)
                    )
                }
            );
            
            var lexer = new LexicalAnalysis();
            var tokens = lexer.RunToEnd(grammar, CreateSourceReader(inputText)).ToArray();

            tokens.Should().NotBeNull();
            CollectionAssert.AreEqual(expectedTokens, tokens.Select(t => t.Name));
        }

        [TestCase("", new string[0])]
        [TestCase("B", new string[0])]
        [TestCase("A", new string[0])]
        [TestCase("AB", new string[0] )]
        [TestCase("ABC", new string[0] )]
        [TestCase("ABBC", new[] { "A" } )]
        [TestCase("AABBC", new[] { "A" } )]
        [TestCase("AAABBC", new[] { "A" } )]
        [TestCase("AAABBBC", new[] { "A" } )]
        [TestCase("AAABBBBC", new[] { "A" } )]
        [TestCase("AAABBBBBC", new string[0] )]
        [TestCase("AAAABBC", new string[0])]
        public void ParseStateWithQuantifierAdvanced(string inputText, string[] expectedTokens)
        {
            var grammar = new Grammar<char, Token>(
                new Rule<char, Token>[] {
                    new Rule<char, Token>(
                        "ruleA", 
                        new IState<char>[] {
                            new CharState('A', Quantifier.Range(1, 3)),
                            new CharState('B', Quantifier.Range(2, 4)),
                            new CharState('C'),
                        }, 
                        match => new AToken(match)
                    )
                }
            );
            
            var lexer = new LexicalAnalysis();
            var tokens = lexer.RunToEnd(grammar, CreateSourceReader(inputText)).ToArray();

            tokens.Should().NotBeNull();
            CollectionAssert.AreEqual(expectedTokens, tokens.Select(t => t.Name));
        }

        [TestCase("", new string[0] )]
        [TestCase("ABC", new[] { "A" } )]
        [TestCase("AB", new[] { "A" } )]
        [TestCase("BC", new[] { "A" } )]
        [TestCase("AC", new[] { "A" } )]
        [TestCase("A", new[] { "A" } )]
        [TestCase("B", new[] { "A" } )]
        [TestCase("C", new[] { "A" } )]
        [TestCase("ABX", new[] { "A" } )]
        [TestCase("BCX", new[] { "A" } )]
        [TestCase("XABC", new string[0] )]
        public void ParseOptionalStates(string inputText, string[] expectedTokens)
        {
            var grammar = new Grammar<char, Token>(
                new Rule<char, Token>[] {
                    new Rule<char, Token>(
                        "ruleA", 
                        new IState<char>[] {
                            new CharState('A', Quantifier.AtMostOnce),
                            new CharState('B', Quantifier.AtMostOnce),
                            new CharState('C', Quantifier.AtMostOnce),
                        }, 
                        match => new AToken(match)
                    )
                }
            );
            
            var lexer = new LexicalAnalysis();
            var tokens = lexer.RunToEnd(grammar, CreateSourceReader(inputText)).ToArray();

            tokens.Should().NotBeNull();
            CollectionAssert.AreEqual(expectedTokens, tokens.Select(t => t.Name));
        }

        [TestCase("ABC", new[] { "A" } )]
        [TestCase("ABBC", new[] { "A" } )]
        [TestCase("ABBCC", new[] { "B" } )]
        public void ParseAmbiguousRulesPickLongest(string inputText, string[] expectedTokens)
        {
            var grammar = new Grammar<char, Token>(
                new Rule<char, Token>[] {
                    new Rule<char, Token>(
                        "ruleX", 
                        new IState<char>[] {
                            new CharState('A', Quantifier.AtLeastOnce),
                            new CharState('B', Quantifier.AtLeast(100)),
                        }, 
                        match => throw new Exception("RuleX should never match")
                    ),
                    new Rule<char, Token>(
                        "ruleA", 
                        new IState<char>[] {
                            new CharState('A', Quantifier.Range(1, 1)),
                            new CharState('B', Quantifier.Range(1, 2)),
                            new CharState('C', Quantifier.Once),
                        }, 
                        match => new AToken(match)
                    ),
                    new Rule<char, Token>(
                        "ruleB", 
                        new IState<char>[] {
                            new CharState('A', Quantifier.Range(1, 2)),
                            new CharState('B', Quantifier.Range(1, 3)),
                            new CharState('C', Quantifier.AtLeastOnce),
                        }, 
                        match => new BToken(match)
                    )
                }
            );
            
            var lexer = new LexicalAnalysis();
            var tokens = lexer.RunToEnd(grammar, CreateSourceReader(inputText)).ToArray();

            tokens.Should().NotBeNull();
            CollectionAssert.AreEqual(expectedTokens, tokens.Select(t => t.Name));
        }

        [TestCase("ABCXY", new[] { "A", "C" } )]
        [TestCase("ABCXZ", new[] { "A" } )]
        [TestCase("ABCDX", new[] { "B" } )]
        public void ParseAmbiguousRulesBacktrackOnFailure(string inputText, string[] expectedTokens)
        {
            var grammar = new Grammar<char, Token>(
                new Rule<char, Token>[] {
                    new Rule<char, Token>(
                        "ruleX", 
                        new IState<char>[] {
                            new CharState('A'),
                            new CharState('X'),
                        }, 
                        match => throw new Exception("RuleX should never match")
                    ),
                    new Rule<char, Token>(
                        "ruleA", 
                        new IState<char>[] {
                            new CharState('A'),
                            new CharState('B'),
                        }, 
                        match => new AToken(match)
                    ),
                    new Rule<char, Token>(
                        "ruleB", 
                        new IState<char>[] {
                            new CharState('A'),
                            new CharState('B'),
                            new CharState('C'),
                            new CharState('D'),
                        }, 
                        match => new BToken(match)
                    ),
                    new Rule<char, Token>(
                        "ruleC", 
                        new IState<char>[] {
                            new CharState('C'),
                            new CharState('X'),
                            new CharState('Y'),
                        }, 
                        match => new CToken(match)
                    )
                }
            );
            
            var lexer = new LexicalAnalysis();
            var tokens = lexer.RunToEnd(grammar, CreateSourceReader(inputText)).ToArray();

            tokens.Should().NotBeNull();
            CollectionAssert.AreEqual(expectedTokens, tokens.Select(t => t.Name));
        }

        [TestCase("ARB", new[] { "A" } )]
        [TestCase("ARRB", new string[0])]
        [TestCase("AB", new string[0])]
        [TestCase("A", new string[0])]
        [TestCase("AR", new string[0])]
        [TestCase("R", new string[0])]
        [TestCase("RB", new string[0])]
        [TestCase("B", new string[0])]
        public void ParseNestedRule(string inputText, string[] expectedTokens)
        {
            var grammar = new Grammar<char, Token>(
                new Rule<char, Token>[] {
                    new Rule<char, Token>(
                        "ruleA", 
                        new IState<char>[] {
                            new CharState('A'),
                            new RuleRefState<char, Token>(
                                "^RR", 
                                new Rule<char, Token>(
                                    "#RR", 
                                    new IState<char>[] {
                                        new CharState('R'),
                                    }, 
                                    match => new AToken(match)
                                ),
                                Quantifier.Once
                            ),
                            new CharState('B'),
                        }, 
                        match => new AToken(match)
                    ),
                }
            );
            
            var lexer = new LexicalAnalysis();
            var tokens = lexer.RunToEnd(grammar, CreateSourceReader(inputText)).ToArray();

            tokens.Should().NotBeNull();
            CollectionAssert.AreEqual(expectedTokens, tokens.Select(t => t.Name));
        }

        [TestCase("ARB", new[] { "A" } )]
        [TestCase("ARRRB", new[] { "A" } )]
        [TestCase("AB", new[] { "A" } )]
        [TestCase("", new string[0])]
        [TestCase("RB", new string[0])]
        [TestCase("R", new string[0])]
        [TestCase("B", new string[0])]
        public void ParseNestedRuleWithQuantifier(string inputText, string[] expectedTokens)
        {
            var grammar = new Grammar<char, Token>(
                new Rule<char, Token>[] {
                    new Rule<char, Token>(
                        "ruleA", 
                        new IState<char>[] {
                            new CharState('A'),
                            new RuleRefState<char, Token>(
                                "^RR", 
                                new Rule<char, Token>(
                                    "#RR", 
                                    new IState<char>[] {
                                        new CharState('R'),
                                    }, 
                                    match => new AToken(match)
                                ),
                                Quantifier.Any
                            ),
                            new CharState('B'),
                        }, 
                        match => new AToken(match)
                    ),
                }
            );
            
            var lexer = new LexicalAnalysis();
            var tokens = lexer.RunToEnd(grammar, CreateSourceReader(inputText)).ToArray();

            tokens.Should().NotBeNull();
            CollectionAssert.AreEqual(expectedTokens, tokens.Select(t => t.Name));
        }

        [TestCase("LXR", new[] { "A" } )]
        [TestCase("LYR", new[] { "A" } )]
        [TestCase("LZR", new[] { "A" } )]
        [TestCase("LXXR", new string[0])]
        [TestCase("LXYR", new string[0])]
        [TestCase("LR", new string[0])]
        [TestCase("L", new string[0])]
        [TestCase("R", new string[0])]
        [TestCase("LX", new string[0])]
        [TestCase("XR", new string[0])]
        [TestCase("X", new string[0])]
        public void ParseNestedGrammar(string inputText, string[] expectedTokens)
        {
            var grammar = new Grammar<char, Token>(
                new Rule<char, Token>[] {
                    new Rule<char, Token>(
                        "ruleA", 
                        new IState<char>[] {
                            new CharState('L'),
                            new ChoiceRefState<char, Token>(
                                "refG", 
                                new Choice<char, Token>(
                                    new Rule<char, Token>[] {
                                        new Rule<char, Token>(
                                            "ruleX", 
                                            new IState<char>[] { new CharState('X') }, 
                                            match => new XToken(match)
                                        ),
                                        new Rule<char, Token>(
                                            "ruleY", 
                                            new IState<char>[] { new CharState('Y') }, 
                                            match => new YToken(match)
                                        ),
                                        new Rule<char, Token>(
                                            "ruleZ", 
                                            new IState<char>[] { new CharState('Z') }, 
                                            match => new ZToken(match)
                                        ),
                                    }
                                ),
                                Quantifier.Once
                            ),
                            new CharState('R'),
                        }, 
                        match => new AToken(match)
                    ),
                }
            );
            
            var lexer = new LexicalAnalysis();
            var tokens = lexer.RunToEnd(grammar, CreateSourceReader(inputText)).ToArray();

            tokens.Should().NotBeNull();
            CollectionAssert.AreEqual(expectedTokens, tokens.Select(t => t.Name));
        }

        [TestCase("LXR", new[] { "A" } )]
        [TestCase("LYR", new[] { "A" } )]
        [TestCase("LZR", new[] { "A" } )]
        [TestCase("LXXR", new[] { "A" } )]
        [TestCase("LXYR", new[] { "A" } )]
        [TestCase("LYZXR", new[] { "A" } )]
        [TestCase("LR", new[] { "A" } )]
        [TestCase("L", new string[0])]
        [TestCase("R", new string[0])]
        [TestCase("LX", new string[0])]
        [TestCase("XR", new string[0])]
        [TestCase("X", new string[0])]
        public void ParseNestedGrammarWithQuantifier(string inputText, string[] expectedTokens)
        {
            // var grammar = FluentLanguage.NewLexicon()
            //     .Rule("ruleA", r => r
            //         .Char('L')
            //         .Choice("g", g => g
            //             .Rule("ruleX", r => r.Char('X'), m => new XToken(m))
            //             .Rule("ruleY", r => r.Char('Y'), m => new YToken(m))
            //             .Rule("ruleZ", r => r.Char('Z'), m => new ZToken(m))
            //         )
            //         .Char('R'),
            //         m => new AToken(m)
            //     )   
            //     .GetGrammar();

            var grammar = new Grammar<char, Token>(
                new Rule<char, Token>[] {
                    new Rule<char, Token>(
                        "ruleA", 
                        new IState<char>[] {
                            new CharState('L'),
                            new ChoiceRefState<char, Token>(
                                "refG", 
                                new Choice<char, Token>(
                                    new Rule<char, Token>[] {
                                        new Rule<char, Token>(
                                            "ruleX", 
                                            new IState<char>[] { new CharState('X') }, 
                                            match => new XToken(match)
                                        ),
                                        new Rule<char, Token>(
                                            "ruleY", 
                                            new IState<char>[] { new CharState('Y') }, 
                                            match => new YToken(match)
                                        ),
                                        new Rule<char, Token>(
                                            "ruleZ", 
                                            new IState<char>[] { new CharState('Z') }, 
                                            match => new ZToken(match)
                                        ),
                                    }
                                ),
                                Quantifier.Any
                            ),
                            new CharState('R'),
                        }, 
                        match => new AToken(match)
                    ),
                }
            );
            
            var lexer = new LexicalAnalysis();
            var tokens = lexer.RunToEnd(grammar, CreateSourceReader(inputText)).ToArray();

            tokens.Should().NotBeNull();
            CollectionAssert.AreEqual(expectedTokens, tokens.Select(t => t.Name));
        }

        private SourceFileReader CreateSourceReader(string sourceText)
        {
            return new SourceFileReader(new ConsoleTrace(), "test.src", new StringReader(sourceText));
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
