using System;
using LLang.Abstractions;
using LLang.Abstractions.Languages;
using NUnit.Framework;
using FluentAssertions;
using System.IO;
using System.Linq;
using LLang.Utilities;

namespace LLang.Tests.Demos.Json
{
    [TestFixture]
    public class JsonGrammarLexicalTests
    {
        [Test]
        public void FlatObject()
        {
            var lexer = new LexicalAnalysis();
            var source = CreateSourceReader(@"{ ""num"": 123, ""str"": ""abc"" }");
            
            var tokens = lexer.RunToEnd(JsonGrammar.CreateLexicon(), source).ToArray();

            CollectionAssert.AreEqual(
                new[] { "OPENOBJ", "WS", "STR", "COLON", "WS", "NUM", "COMMA", "WS", "STR", "COLON", "WS", "STR", "WS", "CLOSEOBJ" },
                tokens.Select(t => t.Name)
            );
        }

        [Test]
        public void FlatArray()
        {
            var lexer = new LexicalAnalysis();
            var source = CreateSourceReader(@"[123, ""abc"", true]");
            
            var tokens = lexer.RunToEnd(JsonGrammar.CreateLexicon(), source).ToArray();

            CollectionAssert.AreEqual(
                new[] { "OPENARR", "NUM", "COMMA", "WS", "STR", "COMMA", "WS", "TRUE", "CLOSEARR" },
                tokens.Select(t => t.Name)
            );
        }

        [Test]
        public void NestedObjectsAndArrays()
        {
            var lexer = new LexicalAnalysis();
            var source = CreateSourceReader(@"{""obj"": {""arr"": [true, false] } }");
            
            var tokens = lexer.RunToEnd(JsonGrammar.CreateLexicon(), source).ToArray();

            CollectionAssert.AreEqual(
                new[] { 
                    "OPENOBJ", "STR", "COLON", "WS", "OPENOBJ",
                    "STR", "COLON", "WS", "OPENARR", 
                    "TRUE", "COMMA", "WS", "FALSE",
                    "CLOSEARR", "WS", "CLOSEOBJ", "WS", "CLOSEOBJ" 
                },
                tokens.Select(t => t.Name)
            );
        }

        [Test]
        public void SignedAndFractionalNumbers()
        {
            var lexer = new LexicalAnalysis();
            var source = CreateSourceReader(@"123.45, -0.5, +1.25");
            
            var tokens = lexer.RunToEnd(JsonGrammar.CreateLexicon(), source).ToArray();

            CollectionAssert.AreEqual(
                new[] { "NUM", "COMMA", "WS", "NUM", "COMMA", "WS", "NUM" },
                tokens.Select(t => t.Name)
            );

            var numberValues = tokens.OfType<JsonGrammar.NumberToken>().Select(t => t.Value).ToArray();
            CollectionAssert.AreEqual(
                new[] { 123.45m, -0.5m, 1.25m },
                numberValues
            );
        }

        [Test]
        public void EscapedStrings()
        {
            var lexer = new LexicalAnalysis();
            var source = CreateSourceReader(@"""before\t\""after\""\r\n"":""A,\u0066,C,D""");
            
            var tokens = lexer.RunToEnd(JsonGrammar.CreateLexicon(), source).ToArray();

            CollectionAssert.AreEqual(
                new[] { "STR", "COLON", "STR" },
                tokens.Select(t => t.Name)
            );

            var stringValues = tokens.OfType<JsonGrammar.StringToken>().Select(t => t.Value).ToArray();
            //TODO: expand escape sequences
            CollectionAssert.AreEqual(
                new[] { 
                    "\"before\\t\\\"after\\\"\\r\\n\"",  
                    "\"A,\\u0066,C,D\""
                },
                stringValues
            );
        }

        private SourceFileReader CreateSourceReader(string sourceText)
        {
            return new SourceFileReader(new ConsoleTrace(), "test.src", new StringReader(sourceText));
        }
    }
}