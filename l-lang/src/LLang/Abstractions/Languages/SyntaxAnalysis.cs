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
            return syntaxOrNone.HasValue ? syntaxOrNone.Value : null;
        }

        public SyntaxNode? Run(
            SourceFileReader reader, 
            Grammar<char, Token> lexicalRules, 
            Grammar<Token, SyntaxNode> syntaxRules,
            Preprocessor? preprocessor = null)
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

            parserTraceSpan.ResultValue(resultSyntax);
            traceSpan.ResultValue("OK");

            return resultSyntax;
        }
    }

    public delegate IEnumerable<Token> Preprocessor(IEnumerable<Token> source);
}
