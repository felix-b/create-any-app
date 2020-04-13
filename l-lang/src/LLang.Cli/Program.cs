using System;
using System.IO;
using LLang.Abstractions.Languages;
using LLang.Demos.Json;
using System.Runtime.Serialization;
using System.Xml;
using LLang.Tracing;
using Stopwatch = System.Diagnostics.Stopwatch;
using LLang.Abstractions;

namespace LLang.Cli
{
    class Program
    {
        static int Main(string[] args)
        {
            RealTrace.InitializeConsole(TraceLevel.None, useColors: !Console.IsOutputRedirected);

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
            var parser = new SyntaxAnalysis();
            var lexicon = JsonGrammar.CreateLexicon();
            var syntax = JsonGrammar.CreateSyntax();
            var preprocessor = JsonGrammar.CreatePreprocessor();

            // for (int i = 0 ; i < 99 ; i++)
            // {
            //     parser.Run(source, lexicon, syntax, preprocessor);
            //     source.ResetTo(new Marker<char>(-1));
            // }

            var watch = Stopwatch.StartNew();
            var parsedSyntax = parser.Run(source, lexicon, syntax, preprocessor);
            var elapsed = watch.Elapsed;

            Console.WriteLine($"Parsed in {elapsed}");
            return parsedSyntax;
        }

        private static SourceFileReader CreateSourceReader(string filePath)
        {
            var fileReader = new StreamReader(filePath);
            return new SourceFileReader(RealTrace.SingleInstance, filePath, fileReader);
        }
    }
}
