using System;

namespace LLang.Tracing
{
    public class NoopTraceSpan : ITraceSpan
    {
        public void Dispose()
        {
        }

        public T ResultValue<T>(T value)
        {
            return value;
        }

        public void Failure(Exception? error = null)
        {
        }

        public void SetHasLogs()
        {
        }

        public int Depth => 0;

        public static readonly ITraceSpan Instance = new NoopTraceSpan();
    }
}
