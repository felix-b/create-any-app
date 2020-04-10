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

            var lexicalAnalysis = new LexicalAnalysis();
            var tokens = lexicalAnalysis.RunToEnd(lexicalRules, reader);
 
            var preprocessedTokens = (preprocessor != null 
                ? preprocessor(tokens)
                : tokens
            ).ToArray();

            using var syntaxTraceSpan = reader.Trace.Span("SyntaxAnalysis");

            var tokenReader = new TokenReader(reader.Trace, preprocessedTokens);
            var syntaxOrNone = Analysis.RunOnce(syntaxRules, tokenReader);
            var resultSyntax = syntaxOrNone.HasValue ? syntaxOrNone.Value : null;

            return syntaxTraceSpan.ResultValue(resultSyntax);
        }
    }

    public delegate IEnumerable<Token> Preprocessor(IEnumerable<Token> source);
}
