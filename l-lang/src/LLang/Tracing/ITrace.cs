using System;
using System.Collections.Generic;
using LLang.Abstractions;

namespace LLang.Tracing
{
    public interface ITrace
    {
        void Debug(string message, Func<ITraceContextBuilder, ITraceContextBuilder>? context = null);
        void Success(string message, Func<ITraceContextBuilder, ITraceContextBuilder>? context = null);
        void Warning(string message, Func<ITraceContextBuilder, ITraceContextBuilder>? context = null);
        void Error(string message, Func<ITraceContextBuilder, ITraceContextBuilder>? context = null);
        ITraceSpan Span(string message, Func<ITraceContextBuilder, ITraceContextBuilder>? context = null);
        TraceLevel Level { get; }
    }

    public interface ITraceSpan : IDisposable
    {
        T ResultValue<T>(T value);
        void Failure(Exception? error = null);
    }

    public interface ITraceContextBuilder
    {
        ITraceContextBuilder Input<TIn>(IInputContext<TIn> input);
        ITraceContextBuilder State<TIn>(IState<TIn> state);
        ITraceContextBuilder Rule<TIn, TOut>(Rule<TIn, TOut> rule);
        ITraceContextBuilder Choice<TIn, TOut>(Choice<TIn, TOut> choice);
        ITraceContextBuilder Product<TOut>(OptionalProduct<TOut> product);
        ITraceContextBuilder Product(object? product);
        ITraceContextBuilder ChoiceRefStateMatch<TIn, TOut>(IChoiceRefStateMatch<TIn, TOut> match);
        ITraceContextBuilder ChoiceMatch<TIn, TOut>(ChoiceMatch<TIn, TOut> match);
        ITraceContextBuilder RuleRefStateMatch<TIn, TOut>(IRuleRefStateMatch<TIn, TOut> match);
        ITraceContextBuilder RuleMatch<TIn, TOut>(RuleMatch<TIn, TOut> match);
        ITraceContextBuilder StateMatch<TIn>(IStateMatch<TIn> match);
        ITraceContextBuilder Result<T>(T value);
        ITraceContextBuilder Add(string name, object? value, bool forceIncludeName = false);
        
        ITraceContextBuilder AddValue<T>(string name, T value) where T : struct
        {
            Add(name, value.ToString(), forceIncludeName: true);
            return this;
        }
    }

    public enum TraceLevel
    {
        None = 0,
        Span = 1,
        Debug = 2,
        Success = 3,
        Warning = 4,
        Error = 5,
        Quiet = 100
    }

    public enum TraceRecordSpanType
    {
        None,
        Start,
        FinishSuccess,
        FinishFailure
    }

    public readonly ref struct TraceRecord
    {
        public TraceRecord(TraceLevel level, string message, IReadOnlyList<string> context, int spanDepth, TraceRecordSpanType spanType)
        {
            Level = level;
            Message = message;
            Context = context;
            SpanDepth = spanDepth;
            SpanType = spanType;
        }

        public TraceLevel Level { get; }
        public string Message { get; }
        public IReadOnlyList<string> Context { get; }
        public int SpanDepth { get; }
        public TraceRecordSpanType SpanType { get; }
    }

    public interface ITraceOutput
    {
        void WriteRecord(ref TraceRecord record);
    }
}
