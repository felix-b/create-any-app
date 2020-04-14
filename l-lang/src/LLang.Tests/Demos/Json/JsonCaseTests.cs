using System;
using LLang.Abstractions;
using LLang.Abstractions.Languages;
using NUnit.Framework;
using FluentAssertions;
using System.IO;
using System.Linq;
using LLang.Demos.Json;
using LLang.Tracing;
using System.Diagnostics;
using System.Xml;
using System.Runtime.Serialization;

namespace LLang.Tests.Demos.Json
{
    [TestFixture]
    public class JsonCaseTests
    {
        [TestCase("0.input.json", "0.output.xml")]
        [TestCase("1.input.json", "1.output.xml")]
        [TestCase("2.input.json", "2.output.xml")]
        public void RunJsonTestCase(string inputFileName, string expectedOutputFileName)
        {
            var analysis = new SyntaxAnalysis();
            var lexicon = JsonGrammar.CreateLexicon();
            var syntax = JsonGrammar.CreateSyntax();
            var preprocessor = JsonGrammar.CreatePreprocessor();
            var input = CreateInputReader(inputFileName);

            var parsedSyntax = analysis.Run(input, lexicon, syntax, preprocessor);
            parsedSyntax.Should().NotBeNull();
            var parsedSemantics = JsonSemantics.CreateFromSyntax(parsedSyntax!);
            var output = SerializeSemanticModel(parsedSemantics);
            
            var expectedOutputText = ReadExpectedOutput(expectedOutputFileName);
            var actualOutputText = ReadActualOutput(output);

            Assert.AreEqual(expectedOutputText, actualOutputText);
        }

        private static SourceFileReader CreateInputReader(string fileName)
        {
            var fileReader = new StreamReader(GetFullPath(fileName));
            return new SourceFileReader(RealTrace.SingleInstance, fileName, fileReader);
        }

        private static MemoryStream SerializeSemanticModel(object? semantics)
        {
            var stream = new MemoryStream();
            var writer = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true });
            var serializer = new DataContractSerializer(typeof(JsonSemantics.JsonNode));

            serializer.WriteObject(writer, semantics);
            
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private static string ReadExpectedOutput(string fileName)
        {
            using (var reader = new StreamReader(GetFullPath(fileName)))
            {
                return reader.ReadToEnd();
            }
        }

        private static string ReadActualOutput(MemoryStream output)
        {
            using (var reader = new StreamReader(output))
            {
                return reader.ReadToEnd();
            }
        }

        private static string GetFullPath(string fileName)
        {
            return Path.Combine(
                TestContext.CurrentContext.TestDirectory, 
                "..", 
                "..",
                "..",
                "Demos",
                "Json",
                "Cases",
                fileName);
        }
    }
}
