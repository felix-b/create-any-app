using System.Collections.Generic;

namespace LLang.Abstractions.Languages
{
    public class LexicalAnalysis
    {
        public IEnumerable<Token> RunToEnd(Grammar<char, Token> language, SourceFileReader reader)
        {
            int tokenCount = 0;

            while (!reader.IsEndOfInput)
            {
                var tokenOrNone = Analysis.RunOnce(language, reader);
                if (tokenOrNone.HasValue)
                {
                    tokenCount++;
                    yield return tokenOrNone.Value;
                }
                else
                {
                    break;
                }
            }

            reader.Trace.Success($"Lexer scan complete, {tokenCount} token(s).");
        }
    }
}
