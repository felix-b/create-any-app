using System.Collections.Generic;

namespace LLang.Abstractions.Languages
{
    public class LexicalAnalysis
    {
        public IEnumerable<Token> RunToEnd(Grammar<char, Token> language, SourceFileReader reader)
        {
            using var trace = reader.Trace.Span("LexicalAnalysis.RunToEnd");

            while (!reader.IsEndOfInput)
            {
                var tokenOrNone = Analysis.RunOnce(language, reader);
                if (tokenOrNone.HasValue)
                {
                    yield return tokenOrNone.Value;
                }
                else
                {
                    break;
                }
            }
        }
    }
}
