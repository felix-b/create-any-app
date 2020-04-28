using LLang.Abstractions;
using LLang.Abstractions.Languages;
using NUnit.Framework;
using FluentAssertions;
using System.IO;
using System.Linq;
using LLang.Demos.Json;
using LLang.Tracing;
using LLang.Tests;
using static LLang.Demos.Json.JsonGrammar;

namespace LLang.Tests.Demos.Json
{
    [TestFixture]
    public class JsonGrammarSyntaxTests
    {
        public static readonly JsonGrammar.SyntaxGrammarKind[] SyntaxGrammarCases = new[] {
            JsonGrammar.SyntaxGrammarKind.NonRecursiveLists,
            JsonGrammar.SyntaxGrammarKind.RecursiveLists
        };

        [TestCaseSource(nameof(SyntaxGrammarCases))]
        public void ScalarValue(JsonGrammar.SyntaxGrammarKind grammarKind)
        {
            var syntax = ParseJsonSyntax(@"""some-text""", grammarKind);

            syntax.DrillAs<ScalarValueSyntax>(scalarValueSyntax => {
                scalarValueSyntax.ScalarToken.Should().BeOfType<StringToken>();
                scalarValueSyntax.ScalarToken.ClrValue.Should().Be(@"some-text");
            });
        }

        [TestCaseSource(nameof(SyntaxGrammarCases))]
        public void EmptyArray(JsonGrammar.SyntaxGrammarKind grammarKind)
        {
            var syntax = ParseJsonSyntax(@"[]", grammarKind);

            syntax.Should().NotBeNull();
            syntax.Should().BeOfType<ArrayValueSyntax>();

            var arraySyntax = ((ArrayValueSyntax)syntax!).ArraySyntax;
            arraySyntax.Should().NotBeNull();
            arraySyntax.Items.Should().BeEmpty();
        }

        [TestCaseSource(nameof(SyntaxGrammarCases))]
        public void FlatSingleItemArray(JsonGrammar.SyntaxGrammarKind grammarKind)
        {
            var syntax = ParseJsonSyntax(@"[123]", grammarKind);

            syntax.Should().NotBeNull();
            syntax.Should().BeOfType<ArrayValueSyntax>();

            var arraySyntax = ((ArrayValueSyntax)syntax!).ArraySyntax;
            arraySyntax.Should().NotBeNull();
            arraySyntax.Items.Count.Should().Be(1);
            arraySyntax.Items[0].Should().BeOfType<ScalarValueSyntax>();
            ((ScalarValueSyntax)arraySyntax.Items[0]).ScalarToken.ClrValue.Should().Be(123);
        }

        [TestCaseSource(nameof(SyntaxGrammarCases))]
        public void FlatTwoItemsArray(JsonGrammar.SyntaxGrammarKind grammarKind)
        {
            var syntax = ParseJsonSyntax(@"[123,true]", grammarKind);

            syntax.Should().NotBeNull();
            syntax.Should().BeOfType<ArrayValueSyntax>();

            var arraySyntax = ((ArrayValueSyntax)syntax!).ArraySyntax;
            arraySyntax.Should().NotBeNull();
            arraySyntax.Items.Count.Should().Be(2);
            arraySyntax.Items.Should().AllBeOfType<ScalarValueSyntax>();
            CollectionAssert.AreEquivalent(
                new object[] { 123, true },
                arraySyntax.Items.OfType<ScalarValueSyntax>().Select(scalar => scalar.ScalarToken.ClrValue)
            );
        }

        [TestCaseSource(nameof(SyntaxGrammarCases))]
        public void FlatThreeItemsArray(JsonGrammar.SyntaxGrammarKind grammarKind)
        {
            var syntax = ParseJsonSyntax(@"[123,true,""abc""]", grammarKind);

            syntax.Should().NotBeNull();
            syntax.Should().BeOfType<ArrayValueSyntax>();

            var arraySyntax = ((ArrayValueSyntax)syntax!).ArraySyntax;
            arraySyntax.Should().NotBeNull();
            arraySyntax.Items.Count.Should().Be(3);
            arraySyntax.Items.Should().AllBeOfType<ScalarValueSyntax>();
            CollectionAssert.AreEquivalent(
                new object[] { 123, true, "abc" }, 
                arraySyntax.Items.OfType<ScalarValueSyntax>().Select(scalar => scalar.ScalarToken.ClrValue)
            );
        }

        [TestCaseSource(nameof(SyntaxGrammarCases))]
        public void EmptyObject(JsonGrammar.SyntaxGrammarKind grammarKind)
        {
            var syntax = ParseJsonSyntax(@"{}", grammarKind);

            syntax.Should().NotBeNull().And.BeOfType<ObjectValueSyntax>();
            var objectSyntax = ((ObjectValueSyntax)syntax!).ObjectSyntax;
            objectSyntax.Should().NotBeNull();
            objectSyntax.Properties.Should().BeEmpty();
        }


        [TestCaseSource(nameof(SyntaxGrammarCases))]
        public void FlatObjectWithSingleProperty(JsonGrammar.SyntaxGrammarKind grammarKind)
        {
            var syntax = ParseJsonSyntax(@"{""num"":123}", grammarKind);

            syntax.Should().NotBeNull().And.BeOfType<ObjectValueSyntax>();
            var objectSyntax = ((ObjectValueSyntax)syntax!).ObjectSyntax;
            objectSyntax.Should().NotBeNull();
            objectSyntax.Properties.Count.Should().Be(1);

            objectSyntax.Properties[0].Name.Should().Be("num"); 
            objectSyntax.Properties[0].ValueSyntax.Should().BeOfType<ScalarValueSyntax>()
               .Which.ScalarToken.ClrValue.Should().Be(123.0m);
        }

        [TestCaseSource(nameof(SyntaxGrammarCases))]
        public void FlatObjectWithTwoProperties(JsonGrammar.SyntaxGrammarKind grammarKind)
        {
            var syntax = ParseJsonSyntax(@"{""num"":123,""str"":""abc""}", grammarKind);

            syntax.Should().NotBeNull().And.BeOfType<ObjectValueSyntax>();
            var objectSyntax = ((ObjectValueSyntax)syntax!).ObjectSyntax;
            objectSyntax.Should().NotBeNull();
            objectSyntax.Properties.Count.Should().Be(2);

            objectSyntax.Properties[0].Name.Should().Be("num");
            objectSyntax.Properties[0].ValueSyntax.Should().BeOfType<ScalarValueSyntax>()
               .Which.ScalarToken.ClrValue.Should().Be(123.0m);

            objectSyntax.Properties[1].Name.Should().Be("str");
            objectSyntax.Properties[1].ValueSyntax.Should().BeOfType<ScalarValueSyntax>()
               .Which.ScalarToken.ClrValue.Should().Be("abc");
        }

        [TestCaseSource(nameof(SyntaxGrammarCases))]
        public void FlatObjectWithThreeProperties(JsonGrammar.SyntaxGrammarKind grammarKind)
        {
            var syntax = ParseJsonSyntax(@"{""num"":123,""str"":""abc"",""yesOrNo"":true}", grammarKind);

            syntax.Should().NotBeNull().And.BeOfType<ObjectValueSyntax>();
            var objectSyntax = ((ObjectValueSyntax)syntax!).ObjectSyntax;
            objectSyntax.Should().NotBeNull();
            objectSyntax.Properties.Count.Should().Be(3);

            objectSyntax.Properties[0].Name.Should().Be("num"); 
            objectSyntax.Properties[0].ValueSyntax.Should().BeOfType<ScalarValueSyntax>()
               .Which.ScalarToken.ClrValue.Should().Be(123.0m);

            objectSyntax.Properties[1].Name.Should().Be("str");
            objectSyntax.Properties[1].ValueSyntax.Should().BeOfType<ScalarValueSyntax>()
               .Which.ScalarToken.ClrValue.Should().Be("abc"); 

            objectSyntax.Properties[2].Name.Should().Be("yesOrNo"); 
            objectSyntax.Properties[2].ValueSyntax.Should().BeOfType<ScalarValueSyntax>()
               .Which.ScalarToken.ClrValue.Should().Be(true); 
        }

        [TestCaseSource(nameof(SyntaxGrammarCases))]
        public void EmptyNestedArray(JsonGrammar.SyntaxGrammarKind grammarKind)
        {
            var syntax = ParseJsonSyntax(@"[[]]", grammarKind);

            syntax.DrillAs<ArrayValueSyntax>().ArraySyntax.Drill(arraySyntax => {
                arraySyntax.Items.Count.Should().Be(1);
                arraySyntax.Items[0].DrillAs<ArrayValueSyntax>().ArraySyntax.Drill(nestedArraySyntax => {
                    nestedArraySyntax.Items.Should().BeEmpty();
                });
            });
        }

        [TestCaseSource(nameof(SyntaxGrammarCases))]
        public void NestedArrayInMiddleItem(JsonGrammar.SyntaxGrammarKind grammarKind)
        {
            var syntax = ParseJsonSyntax(@"[11,[22,33],44]", grammarKind);

            syntax.DrillAs<ArrayValueSyntax>().ArraySyntax.Drill(arraySyntax => {
                arraySyntax.Items.Count.Should().Be(3);
                arraySyntax.Items[0].DrillAs<ScalarValueSyntax>(scalarValueSyntax => {
                    scalarValueSyntax.ScalarToken.DrillAs<NumberToken>(numberToken => 
                        numberToken.Value.Should().Be(11.0m)
                    );
                });
                arraySyntax.Items[1].DrillAs<ArrayValueSyntax>().ArraySyntax.Drill(arraySyntax => {
                    arraySyntax.Items.Count.Should().Be(2);
                    var itemValues = arraySyntax.Items
                        .Select(item => item.DrillAs<ScalarValueSyntax>().ScalarToken.DrillAs<NumberToken>().Value)
                        .ToArray();
                    CollectionAssert.AreEqual(new[] { 22.0m, 33.0m }, itemValues);
                });
                arraySyntax.Items[2].DrillAs<ScalarValueSyntax>(scalarValueSyntax => {
                    scalarValueSyntax.ScalarToken.DrillAs<NumberToken>(numberToken => 
                        numberToken.Value.Should().Be(44.0m)
                    );
                });
            });
        }

        [TestCaseSource(nameof(SyntaxGrammarCases))]
        public void NestedArrayInFirstItem(JsonGrammar.SyntaxGrammarKind grammarKind)
        {
            var syntax = ParseJsonSyntax(@"[[11,22],33]", grammarKind);

            syntax.DrillAs<ArrayValueSyntax>().ArraySyntax.Drill(arraySyntax => {
                arraySyntax.Items.Count.Should().Be(2);
                arraySyntax.Items[0].DrillAs<ArrayValueSyntax>().ArraySyntax.Drill(arraySyntax => {
                    arraySyntax.Items.Count.Should().Be(2);
                    var itemValues = arraySyntax.Items
                        .Select(item => item.DrillAs<ScalarValueSyntax>().ScalarToken.DrillAs<NumberToken>().Value)
                        .ToArray();
                    CollectionAssert.AreEqual(new[] { 11.0m, 22.0m }, itemValues);
                });
                arraySyntax.Items[1].DrillAs<ScalarValueSyntax>(scalarValueSyntax => {
                    scalarValueSyntax.ScalarToken.DrillAs<NumberToken>(numberToken => 
                        numberToken.Value.Should().Be(33.0m)
                    );
                });
            });
        }

        [TestCaseSource(nameof(SyntaxGrammarCases))]
        public void NestedArrayInLastItem(JsonGrammar.SyntaxGrammarKind grammarKind)
        {
            var syntax = ParseJsonSyntax(@"[11,[22,33]]", grammarKind);

            syntax.DrillAs<ArrayValueSyntax>().ArraySyntax.Drill(arraySyntax => {
                arraySyntax.Items.Count.Should().Be(2);
                arraySyntax.Items[0].DrillAs<ScalarValueSyntax>(scalarValueSyntax => {
                    scalarValueSyntax.ScalarToken.DrillAs<NumberToken>(numberToken => 
                        numberToken.Value.Should().Be(11.0m)
                    );
                });
                arraySyntax.Items[1].DrillAs<ArrayValueSyntax>().ArraySyntax.Drill(arraySyntax => {
                    arraySyntax.Items.Count.Should().Be(2);
                    var itemValues = arraySyntax.Items
                        .Select(item => item.DrillAs<ScalarValueSyntax>().ScalarToken.DrillAs<NumberToken>().Value)
                        .ToArray();
                    CollectionAssert.AreEqual(new[] { 22.0m, 33.0m }, itemValues);
                });
            });
        }

        [TestCaseSource(nameof(SyntaxGrammarCases))]
        public void EmptyNestedObject(JsonGrammar.SyntaxGrammarKind grammarKind)
        {
            var syntax = ParseJsonSyntax(@"{""p1"":{}}", grammarKind);

            syntax.Should().NotBeNull();

            syntax.DrillAs<ObjectValueSyntax>().ObjectSyntax.Drill(objectSyntax => {
                objectSyntax.Properties.Count.Should().Be(1);

                objectSyntax.Properties[0].Drill(propertySyntax => {
                    propertySyntax.Name.Should().Be("p1");
                    propertySyntax.ValueSyntax.DrillAs<ObjectValueSyntax>().ObjectSyntax.Drill(nestedObjectSyntax => {
                        nestedObjectSyntax.Properties.Should().BeEmpty();
                    });
                });
            });
        }

        [TestCaseSource(nameof(SyntaxGrammarCases))]
        public void NestedObjectInMiddleProperty(JsonGrammar.SyntaxGrammarKind grammarKind)
        {
            var syntax = ParseJsonSyntax(@"{""p1"":11,""p2"":{""p3"":33,""p4"":44},""p5"":55}", grammarKind);

            syntax.Should().NotBeNull();

            syntax.DrillAs<ObjectValueSyntax>().ObjectSyntax.Drill(objectSyntax => {
                objectSyntax.Properties.Count.Should().Be(3);

                objectSyntax.Properties[0].Drill(propertySyntax => {
                    propertySyntax.Name.Should().Be("p1");
                    propertySyntax.ValueSyntax.DrillAs<ScalarValueSyntax>(scalarValueSyntax => {
                        scalarValueSyntax.ScalarToken.DrillAs<NumberToken>(numberToken => 
                            numberToken.Value.Should().Be(11.0m)
                        );
                    });
                });

                objectSyntax.Properties[1].Drill(propertySyntax => {
                    propertySyntax.Name.Should().Be("p2");
                    propertySyntax.ValueSyntax.DrillAs<ObjectValueSyntax>().ObjectSyntax.Drill(nestedObjectSyntax => {
                        nestedObjectSyntax.Properties.Count.Should().Be(2);

                        nestedObjectSyntax.Properties[0].Drill(nestedPropertySyntax => {
                            nestedPropertySyntax.Name.Should().Be("p3");
                            nestedPropertySyntax.ValueSyntax.DrillAs<ScalarValueSyntax>(scalarValueSyntax => {
                                scalarValueSyntax.ScalarToken.DrillAs<NumberToken>(numberToken => 
                                    numberToken.Value.Should().Be(33.0m)
                                );
                            });
                        });

                        nestedObjectSyntax.Properties[1].Drill(nestedPropertySyntax => {
                            nestedPropertySyntax.Name.Should().Be("p4");
                            nestedPropertySyntax.ValueSyntax.DrillAs<ScalarValueSyntax>(scalarValueSyntax => {
                                scalarValueSyntax.ScalarToken.DrillAs<NumberToken>(numberToken => 
                                    numberToken.Value.Should().Be(44.0m)
                                );
                            });
                        });
                    });
                });

                objectSyntax.Properties[2].Drill(propertySyntax => {
                    propertySyntax.Name.Should().Be("p5");
                    propertySyntax.ValueSyntax.DrillAs<ScalarValueSyntax>(scalarValueSyntax => {
                        scalarValueSyntax.ScalarToken.DrillAs<NumberToken>(numberToken => 
                            numberToken.Value.Should().Be(55.0m)
                        );
                    });
                });
            });
        }

        [TestCaseSource(nameof(SyntaxGrammarCases))]
        public void NestedObjectInFirstProperty(JsonGrammar.SyntaxGrammarKind grammarKind)
        {
            var syntax = ParseJsonSyntax(@"{""p1"":{""p2"":22},""p3"":33}", grammarKind);

            syntax.Should().NotBeNull();

            syntax.DrillAs<ObjectValueSyntax>().ObjectSyntax.Drill(objectSyntax => {
                objectSyntax.Properties.Count.Should().Be(2);

                objectSyntax.Properties[0].Drill(propertySyntax => {
                    propertySyntax.Name.Should().Be("p1");
                    propertySyntax.ValueSyntax.DrillAs<ObjectValueSyntax>().ObjectSyntax.Drill(nestedObjectSyntax => {
                        nestedObjectSyntax.Properties.Count.Should().Be(1);
                        nestedObjectSyntax.Properties[0].Drill(nestedPropertySyntax => {
                            nestedPropertySyntax.Name.Should().Be("p2");
                            nestedPropertySyntax.ValueSyntax.DrillAs<ScalarValueSyntax>(scalarValueSyntax => {
                                scalarValueSyntax.ScalarToken.DrillAs<NumberToken>(numberToken => 
                                    numberToken.Value.Should().Be(22.0m)
                                );
                            });
                        });
                    });
                });

                objectSyntax.Properties[1].Drill(propertySyntax => {
                    propertySyntax.Name.Should().Be("p3");
                    propertySyntax.ValueSyntax.DrillAs<ScalarValueSyntax>(scalarValueSyntax => {
                        scalarValueSyntax.ScalarToken.DrillAs<NumberToken>(numberToken => 
                            numberToken.Value.Should().Be(33.0m)
                        );
                    });
                });
            });
        }

        [TestCaseSource(nameof(SyntaxGrammarCases))]
        public void NestedObjectInLastProperty(JsonGrammar.SyntaxGrammarKind grammarKind)
        {
            var syntax = ParseJsonSyntax(@"{""p1"":11,""p2"":{""p3"":33}}", grammarKind);

            syntax.Should().NotBeNull();

            syntax.DrillAs<ObjectValueSyntax>().ObjectSyntax.Drill(objectSyntax => {
                objectSyntax.Properties.Count.Should().Be(2);

                objectSyntax.Properties[0].Drill(propertySyntax => {
                    propertySyntax.Name.Should().Be("p1");
                    propertySyntax.ValueSyntax.DrillAs<ScalarValueSyntax>(scalarValueSyntax => {
                        scalarValueSyntax.ScalarToken.DrillAs<NumberToken>(numberToken => 
                            numberToken.Value.Should().Be(11.0m)
                        );
                    });
                });

                objectSyntax.Properties[1].Drill(propertySyntax => {
                    propertySyntax.Name.Should().Be("p2");
                    propertySyntax.ValueSyntax.DrillAs<ObjectValueSyntax>().ObjectSyntax.Drill(nestedObjectSyntax => {
                        nestedObjectSyntax.Properties.Count.Should().Be(1);
                        nestedObjectSyntax.Properties[0].Drill(nestedPropertySyntax => {
                            nestedPropertySyntax.Name.Should().Be("p3");
                            nestedPropertySyntax.ValueSyntax.DrillAs<ScalarValueSyntax>(scalarValueSyntax => {
                                scalarValueSyntax.ScalarToken.DrillAs<NumberToken>(numberToken => 
                                    numberToken.Value.Should().Be(33.0m)
                                );
                            });
                        });
                    });
                });
            });
        }

        private SyntaxNode? ParseJsonSyntax(string json, JsonGrammar.SyntaxGrammarKind syntaxKind)
        {
            var parser = new SyntaxAnalysis();
            var source = CreateSourceReader(json);
            
            var syntax = parser.Run(
                source, 
                JsonGrammar.CreateLexicon(), 
                JsonGrammar.CreateSyntax(syntaxKind), 
                JsonGrammar.CreatePreprocessor(),
                out var diagnostics);

            diagnostics.Count.Should().Be(0);
            return syntax;
        }

        private SourceFileReader CreateSourceReader(string sourceText)
        {
            return new SourceFileReader(new NoopTrace(), "test.src", new StringReader(sourceText));
        }
    }
}
