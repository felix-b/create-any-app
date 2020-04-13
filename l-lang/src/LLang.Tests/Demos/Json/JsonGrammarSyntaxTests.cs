using System;
using LLang.Abstractions;
using LLang.Abstractions.Languages;
using NUnit.Framework;
using FluentAssertions;
using System.IO;
using System.Linq;
using LLang.Demos.Json;
using static LLang.Demos.Json.JsonGrammar;
using LLang.Tracing;

namespace LLang.Tests.Demos.Json
{
    [TestFixture]
    public class JsonGrammarSyntaxTests
    {
        [Test]
        public void ScalarValue()
        {
            var syntax = ParseJsonSyntax(@"""some-text""");

            syntax.DrillAs<ScalarValueSyntax>(scalarValueSyntax => {
                scalarValueSyntax.ScalarToken.Should().BeOfType<StringToken>();
                scalarValueSyntax.ScalarToken.ClrValue.Should().Be(@"some-text");
            });
        }

        [Test]
        public void EmptyArray()
        {
            var syntax = ParseJsonSyntax(@"[]");

            syntax.Should().NotBeNull();
            syntax.Should().BeOfType<ArrayValueSyntax>();

            var arraySyntax = ((ArrayValueSyntax)syntax!).ArraySyntax;
            arraySyntax.Should().NotBeNull();
            arraySyntax.Items.Should().BeEmpty();
        }

        [Test]
        public void FlatSingleItemArray()
        {
            var syntax = ParseJsonSyntax(@"[123]");

            syntax.Should().NotBeNull();
            syntax.Should().BeOfType<ArrayValueSyntax>();

            var arraySyntax = ((ArrayValueSyntax)syntax!).ArraySyntax;
            arraySyntax.Should().NotBeNull();
            arraySyntax.Items.Count.Should().Be(1);
            arraySyntax.Items[0].Should().BeOfType<ScalarValueSyntax>();
            ((ScalarValueSyntax)arraySyntax.Items[0]).ScalarToken.ClrValue.Should().Be(123);
        }

        [Test]
        public void FlatTwoItemsArray()
        {
            var syntax = ParseJsonSyntax(@"[123,true]");

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

        [Test]
        public void FlatThreeItemsArray()
        {
            var syntax = ParseJsonSyntax(@"[123,true,""abc""]");

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

        [Test]
        public void EmptyObject()
        {
            var syntax = ParseJsonSyntax(@"{}");

            syntax.Should().NotBeNull().And.BeOfType<ObjectValueSyntax>();
            var objectSyntax = ((ObjectValueSyntax)syntax!).ObjectSyntax;
            objectSyntax.Should().NotBeNull();
            objectSyntax.Properties.Should().BeEmpty();
        }


        [Test]
        public void FlatObjectWithSingleProperty()
        {
            var syntax = ParseJsonSyntax(@"{""num"":123}");

            syntax.Should().NotBeNull().And.BeOfType<ObjectValueSyntax>();
            var objectSyntax = ((ObjectValueSyntax)syntax!).ObjectSyntax;
            objectSyntax.Should().NotBeNull();
            objectSyntax.Properties.Count.Should().Be(1);

            objectSyntax.Properties[0].Name.Should().Be("num"); 
            objectSyntax.Properties[0].ValueSyntax.Should().BeOfType<ScalarValueSyntax>()
               .Which.ScalarToken.ClrValue.Should().Be(123.0m);
        }

        [Test]
        public void FlatObjectWithTwoProperties()
        {
            var syntax = ParseJsonSyntax(@"{""num"":123,""str"":""abc""}");

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

        [Test]
        public void FlatObjectWithThreeProperties()
        {
            var syntax = ParseJsonSyntax(@"{""num"":123,""str"":""abc"",""yesOrNo"":true}");

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

        [Test]
        public void EmptyNestedArray()
        {
            var syntax = ParseJsonSyntax(@"[[]]");

            syntax.DrillAs<ArrayValueSyntax>().ArraySyntax.Drill(arraySyntax => {
                arraySyntax.Items.Count.Should().Be(1);
                arraySyntax.Items[0].DrillAs<ArrayValueSyntax>().ArraySyntax.Drill(nestedArraySyntax => {
                    nestedArraySyntax.Items.Should().BeEmpty();
                });
            });
        }

        [Test]
        public void NestedArrayInMiddleItem()
        {
            var syntax = ParseJsonSyntax(@"[11,[22,33],44]");

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

        [Test]
        public void NestedArrayInFirstItem()
        {
            var syntax = ParseJsonSyntax(@"[[11,22],33]");

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

        [Test]
        public void NestedArrayInLastItem()
        {
            var syntax = ParseJsonSyntax(@"[11,[22,33]]");

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

        [Test]
        public void EmptyNestedObject()
        {
            var syntax = ParseJsonSyntax(@"{""p1"":{}}");

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

        [Test]
        public void NestedObjectInMiddleProperty()
        {
            var syntax = ParseJsonSyntax(@"{""p1"":11,""p2"":{""p3"":33,""p4"":44},""p5"":55}");

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

        [Test]
        public void NestedObjectInFirstProperty()
        {
            var syntax = ParseJsonSyntax(@"{""p1"":{""p2"":22},""p3"":33}");

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

        [Test]
        public void NestedObjectInLastProperty()
        {
            var syntax = ParseJsonSyntax(@"{""p1"":11,""p2"":{""p3"":33}}");

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

        private SyntaxNode? ParseJsonSyntax(string json)
        {
            var parser = new SyntaxAnalysis();
            var source = CreateSourceReader(json);
            
            var syntax = parser.Run(
                source, 
                JsonGrammar.CreateLexicon(), 
                JsonGrammar.CreateSyntax(), 
                JsonGrammar.CreatePreprocessor());

            return syntax;
        }

        private SourceFileReader CreateSourceReader(string sourceText)
        {
            return new SourceFileReader(new NoopTrace(), "test.src", new StringReader(sourceText));
        }
    }
}

public static class PropDrillingExtensions
{
    public static T DrillAs<T>(this object? value, Action<T>? assert = null, string? because = null)
        where T : class
    {
        value.Should().NotBeNull(because);
        value.Should().BeOfType<T>(because);
        T drilled = (T)value!;
        assert?.Invoke(drilled);
        return drilled;
    }

    public static T Drill<T>(this T value, Action<T> assert)
    {
        assert(value);
        return value;
    }
}
