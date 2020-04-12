using System;
using System.IO;
using LLang.Abstractions;
using LLang.Abstractions.Languages;
using LLang.Utilities;
using LLang.Demos.Json;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Xml;
using PostSharp.Patterns.Diagnostics;
using PostSharp.Patterns.Diagnostics.Backends.Console;
using LLang.Tracing;

namespace LLang.Cli
{
    [Log(AttributeExclude = true)]
    class Program
    {
        static int Main(string[] args)
        {
            AnalysisTrace.Initialize(useColors: !Console.IsOutputRedirected);

            if (args.Length != 2)
            {
                Console.WriteLine("Usage: llang <input_json> <output_xml>");
                return 1;
            }

            SourceFileReader input;
            XmlWriter output;

            try
            {
                input = CreateSourceReader(filePath: args[0]);
                output = XmlWriter.Create(outputFileName: args[1], new XmlWriterSettings { Indent = true });
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
                return 1;
            }

            try
            {
                var syntax = ParseJsonSyntax(input);
                if (syntax == null)
                {
                    Console.WriteLine("Syntax error in JSON");
                    return 3;
                }

                var semantics = JsonSemantics.CreateFromSyntax(syntax);
                var serializer = new DataContractSerializer(typeof(JsonSemantics.JsonNode));
                serializer.WriteObject(output, semantics);
                output.Flush();
            }
            catch (Exception e)
            {
                Console.WriteLine("FAILURE!");
                Console.WriteLine(e);
                return 2;
            }

            return 0;
        }

        private static SyntaxNode? ParseJsonSyntax(SourceFileReader source)
        {
            var watch = Stopwatch.StartNew();

            var parser = new SyntaxAnalysis();
            var syntax = parser.Run(
                source, 
                JsonGrammar.CreateLexicon(), 
                JsonGrammar.CreateSyntax(), 
                JsonGrammar.CreatePreprocessor());

            Console.WriteLine($"Parsed in {watch.ElapsedMilliseconds} ms");
            return syntax;
        }

        private static SourceFileReader CreateSourceReader(string filePath)
        {
            var fileReader = new StreamReader(filePath);
            return new SourceFileReader(new NoopTrace(), filePath, fileReader);
        }
    }
}
