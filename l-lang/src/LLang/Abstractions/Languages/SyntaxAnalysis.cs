using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System;

namespace LLang.Abstractions.Languages
{
    public class SyntaxAnalysis
    {
        public SyntaxNode? Run(TokenReader reader, Grammar<Token, SyntaxNode> syntaxRules)
        {
            var syntaxOrNone = Analysis.RunOnce(syntaxRules, reader);
            reader.CheckForFailures();
            return syntaxOrNone.HasValue ? syntaxOrNone.Value : null;
        }

        public SyntaxNode? Run(
            SourceFileReader reader, 
            Grammar<char, Token> lexicalRules, 
            Grammar<Token, SyntaxNode> syntaxRules,
            Preprocessor? preprocessor,
            out IReadOnlyList<Diagnostic> diagnostics)
        {
            using var traceSpan = reader.Trace.Span("SyntaxAnalysis.Run");
            var lexerTraceSpan = reader.Trace.Span("LexicalAnalysis");

            var lexicalAnalysis = new LexicalAnalysis();
            var tokens = lexicalAnalysis.RunToEnd(lexicalRules, reader);
            var preprocessedTokens = (preprocessor != null 
                ? preprocessor(tokens)
                : tokens
            ).ToArray();

            lexerTraceSpan.ResultValue($"{preprocessedTokens.Length} token(s) after preprocess");
            lexerTraceSpan.Dispose();
            using var parserTraceSpan = reader.Trace.Span("SyntaxAnalysis");

            var tokenReader = new TokenReader(reader.Trace, preprocessedTokens);
            var syntaxOrNone = Analysis.RunOnce(syntaxRules, tokenReader);
            var resultSyntax = syntaxOrNone.HasValue ? syntaxOrNone.Value : null;
            tokenReader.CheckForFailures();

            parserTraceSpan.ResultValue(resultSyntax);
            traceSpan.ResultValue("OK");
            
            diagnostics = ConcatAllDiagnostics(reader, tokenReader);
            return resultSyntax;
        }

        public static readonly SyntaxDiagnosticDescription UnexpectedTokenError = new SyntaxDiagnosticDescription(
            code: "LL002", 
            DiagnosticLevel.Error, 
            formatter: diagnostic => $"Unexpected token: '{diagnostic.Input.Span.GetText()}'");

        private static IReadOnlyList<Diagnostic> ConcatAllDiagnostics(SourceFileReader sourceReader, TokenReader tokenReader)
        {
            return sourceReader.Diagnostics.Cast<Diagnostic>().Concat(tokenReader.Diagnostics.Cast<Diagnostic>()).ToList();
        }
    }

    public delegate IEnumerable<Token> Preprocessor(IEnumerable<Token> source);
}
