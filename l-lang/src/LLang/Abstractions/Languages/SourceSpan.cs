using System;
using System.Collections.Generic;
using System.Linq;

namespace LLang.Abstractions.Languages
{
    public class SourceSpan
    {
        private readonly IInputContext<char>? _context;

        public SourceSpan(IMatch<char> match, IInputContext<char>? context = null)
        {
            Start = match.StartMarker;
            End = match.EndMarker;
            _context = context;
        }

        private SourceSpan(Marker<char> start, Marker<char> end)
        {
            Start = start;
            End = end;    
            _context = null;
        }

        private SourceSpan(SourceSpan first, SourceSpan last)
        {
            Start = first.Start;
            End = last.End;    
            _context = first._context ?? last._context;
        }

        public string GetText(int startOffset = 0, int endOffset = 0)
        {
            return _context != null 
                ? _context.GetSlice(Start, End, startOffset, endOffset).ToString()
                : string.Empty;
        }

        public Marker<char> Start { get; }
        public Marker<char> End { get; }
        public int Length => End - Start;
        public bool IsEmpty => Start.Value == -1 && End.Value == -1;

        public static readonly SourceSpan Empty = new SourceSpan(new Marker<char>(-1), new Marker<char>(-1));

        public static SourceSpan Union(IEnumerable<SourceSpan> spans) 
        {
            var (start, end) = spans.Aggregate(
                (new Marker<char>(0), new Marker<char>(0)), 
                (result, span) => span.IsEmpty
                    ? result
                    : (
                        span.Start < result.Item1 ? span.Start : result.Item1, 
                        span.End > result.Item2 ? span.End : result.Item2 
                    )
            );

            return new SourceSpan(start, end);
        }

        public static SourceSpan FromTokens(IMatch<Token> match, IInputContext<Token> context)
        {
            var tokenSlice = context.GetSlice(match.StartMarker, match.EndMarker);
            var firstSpan = tokenSlice.Span[0].Span;
            var lastSpan = tokenSlice.Span[^1].Span;
            return new SourceSpan(firstSpan, lastSpan);
        }

        // public static Span<TIn> Include(IReadOnlyList<SyntaxNode> nodes) 
        // {
        //     return nodes.Count switch {
        //         0 => Empty,
        //         1 => nodes[0].Span,
        //         _ => new Span(
        //             start: nodes[0].Span.Start, 
        //             end: nodes[^1].Span.End)
        //     };
        // }
    }
}

