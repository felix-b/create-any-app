namespace LLang.Tracing
{
    public class NoopTraceOutput : ITraceOutput
    {
        public void WriteRecord(ref TraceRecord record)
        {
        }
    }
}
