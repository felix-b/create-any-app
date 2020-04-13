using System;

namespace LLang.Tracing
{
    public class NoopTrace : ITrace
    {
        public void Debug(string message, Func<ITraceContextBuilder, ITraceContextBuilder>? context = null)
        {
        }

        public void Success(string message, Func<ITraceContextBuilder, ITraceContextBuilder>? context = null)
        {
        }

        public void Warning(string message, Func<ITraceContextBuilder, ITraceContextBuilder>? context = null)
        {
        }

        public void Error(string message, Func<ITraceContextBuilder, ITraceContextBuilder>? context = null)
        {
        }

        public ITraceSpan Span(string message, Func<ITraceContextBuilder, ITraceContextBuilder>? context = null)
        {
            return NoopTraceSpan.Instance;
        }

        public TraceLevel Level => TraceLevel.None;
    }
}
