using System;
using System.Collections.Generic;
using System.Text;
using LLang.Abstractions;

namespace LLang.Utilities
{
    public class NoopTrace : ITrace
    {
        private readonly NoopTraceSpan _noopSpan = new NoopTraceSpan();

        public void Debug(string message, Func<ITraceContext, ITraceContext>? context = null)
        {
        }

        public void Warning(string message, Func<ITraceContext, ITraceContext>? context = null)
        {
        }

        public void Error(string message, Func<ITraceContext, ITraceContext>? context = null)
        {
        }

        public ITraceSpan Span(string message, Func<ITraceContext, ITraceContext>? context = null)
        {
            return _noopSpan;
        }

        private class NoopTraceSpan : ITraceSpan
        {
            public void Dispose()
            {
            }

            public T ResultValue<T>(T value)
            {
                return value;
            }

            public void SetHasLogs()
            {
            }

            public string Message => string.Empty;
            public bool HasLogs => false;
            public bool HasResult => false;
            public object? Result => null;
        }
    }
}
