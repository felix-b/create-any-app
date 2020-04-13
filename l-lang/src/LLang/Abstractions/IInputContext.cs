using System;
using LLang.Tracing;

namespace LLang.Abstractions
{
    public interface IInputContext<TIn>
    {
        Marker<TIn> Mark();
        ReadOnlyMemory<TIn> GetSlice(
            Marker<TIn> start, 
            Marker<TIn> end, 
            int startOffset = 0, 
            int endOffset = 0);
        bool IsEndOfInput { get; }
        bool HasInput { get; }
        TIn Input { get; }
        ITrace Trace { get; }
    }
}
