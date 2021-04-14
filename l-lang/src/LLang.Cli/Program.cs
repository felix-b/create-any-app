using System;
using System.IO;
using System.Linq;
using LLang.Abstractions.Languages;
using LLang.Demos.Json;
using System.Runtime.Serialization;
using System.Xml;
using LLang.Tracing;
using Stopwatch = System.Diagnostics.Stopwatch;
using LLang.Abstractions;
using System.Collections.Generic;

namespace LLang.Cli
{
    static class Program
    {
        private static readonly ConsoleTraceOutput _consoleOutput = new ConsoleTraceOutput(useColors: !Console.IsOutputRedirected);

        static int Main(string[] args)
        {
            RealTrace.InitializeOutput(_consoleOutput, TraceLevel.None);

            if (args.Length < 2 || args.Length > 3 || (args.Length == 3 && args[2] != "-alt"))
            {
                Console.WriteLine("Usage: llang <input_json> <output_xml> [--alt]");
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
                var grammarKind = args.Length == 3 && args[2] == "--alt" 
                    ? JsonGrammar.SyntaxGrammarKind.RecursiveLists 
                    : JsonGrammar.SyntaxGrammarKind.NonRecursiveLists;
                var syntax = ParseJsonSyntax(input, grammarKind);
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

        private static SyntaxNode? ParseJsonSyntax(SourceFileReader source, JsonGrammar.SyntaxGrammarKind grammarKind)
        {
            var parser = new SyntaxAnalysis();
            var lexicon = JsonGrammar.CreateLexicon();
            var syntax = JsonGrammar.CreateSyntax(grammarKind);
            var preprocessor = JsonGrammar.CreatePreprocessor();

            // for (int i = 0 ; i < 99 ; i++)
            // {
            //     parser.Run(source, lexicon, syntax, preprocessor);
            //     source.ResetTo(new Marker<char>(-1));
            // }

            var watch = Stopwatch.StartNew();
            var parsedSyntax = parser.Run(source, lexicon, syntax, preprocessor, out var diagnostics);
            var elapsed = watch.Elapsed;

            Console.WriteLine($"Parsed in {elapsed}");
            PrintDiagnostics(diagnostics, source);

            return parsedSyntax;
        }

        private static SourceFileReader CreateSourceReader(string filePath)
        {
            var fileReader = new StreamReader(filePath);
            return new SourceFileReader(RealTrace.SingleInstance, filePath, fileReader);
        }

        private static void PrintDiagnostics(IEnumerable<Diagnostic> diagnostics, SourceFileReader reader)
        {
            var errorCount = PrintLevel(DiagnosticLevel.Error, ConsoleColor.Red);
            var warningCount = PrintLevel(DiagnosticLevel.Warning, ConsoleColor.Yellow);

            _consoleOutput.ColorPrintLine(errorCount > 0 ? ConsoleColor.Red : ConsoleColor. Gray, $"{errorCount,5} Error(s)");
            _consoleOutput.ColorPrintLine(warningCount > 0 ? ConsoleColor.Yellow : ConsoleColor.Gray, $"{warningCount,5} Warning(s)");

            int PrintLevel(DiagnosticLevel level, ConsoleColor color)
            {
                var count = 0;
                _consoleOutput.UsingColorDo(color, () => {
                    foreach (var singleDiagnostic in diagnostics!.Where(d => d.Description.Level == level))
                    {
                        count++;
                        var location = FormatLocation(singleDiagnostic);
                        Console.WriteLine($"{location}: ({singleDiagnostic.Description.Code}) {singleDiagnostic}");
                    }
                });
                return count;
            }
            
            string FormatLocation(Diagnostic diagnostic)
            {
                var marker = diagnostic switch 
                {
                    Diagnostic<char> c => c.Marker,
                    Diagnostic<Token> t => t.Input != null ? t.Input.Span.Start : new Marker<char>(-1),
                    _ => throw new ArgumentException("Unexpected type of diagnostic", nameof(diagnostic))
                };

                var location = reader.GetLocation(marker);
                return $"{location.FilePath} ({location.Line}:{location.Column})";
            }
        }
    }
}
