using System;
using System.Collections.Generic;
using LLang.Abstractions;

namespace LLang.Tracing
{
    public class RealTrace : ITrace
    {
        private readonly Stack<TraceSpan> _activeSpans = new Stack<TraceSpan>(); 
        private ITraceOutput _output;

        private RealTrace(ITraceOutput output, TraceLevel level)
        {
            _output = output;
            Level = level;
        }

        public void Debug(string message, Func<ITraceContextBuilder, ITraceContextBuilder>? context = null)
        {
            if (Level > TraceLevel.Debug)
            {
                return;
            }

            var record = new TraceRecord(
                TraceLevel.Debug, 
                message, 
                GetContextValues(context), 
                spanDepth: _activeSpans.Count,
                TraceRecordSpanType.None);
            _output.WriteRecord(ref record);
        }

        public void Success(string message, Func<ITraceContextBuilder, ITraceContextBuilder>? context = null)
        {
            if (Level > TraceLevel.Success)
            {
                return;
            }

            var record = new TraceRecord(
                TraceLevel.Success, 
                message, 
                GetContextValues(context), 
                spanDepth: _activeSpans.Count,
                TraceRecordSpanType.None);
            _output.WriteRecord(ref record);
        }

        public void Warning(string message, Func<ITraceContextBuilder, ITraceContextBuilder>? context = null)
        {
            if (Level > TraceLevel.Warning)
            {
                return;
            }

            var record = new TraceRecord(
                TraceLevel.Warning, 
                message, 
                GetContextValues(context), 
                spanDepth: _activeSpans.Count,
                TraceRecordSpanType.None);
            _output.WriteRecord(ref record);
        }

        public void Error(string message, Func<ITraceContextBuilder, ITraceContextBuilder>? context = null)
        {
            if (Level > TraceLevel.Error)
            {
                return;
            }

            var record = new TraceRecord(
                TraceLevel.Error, 
                message, 
                GetContextValues(context), 
                spanDepth: _activeSpans.Count,
                TraceRecordSpanType.None);
            _output.WriteRecord(ref record);
        }

        public ITraceSpan Span(string message, Func<ITraceContextBuilder, ITraceContextBuilder>? context = null)
        {
            if (Level > TraceLevel.Span)
            {
                return NoopTraceSpan.Instance;
            }

            var record = new TraceRecord(
                TraceLevel.Span, 
                message, 
                GetContextValues(context), 
                spanDepth: _activeSpans.Count,
                TraceRecordSpanType.Start);
            _output.WriteRecord(ref record);

            var span = new TraceSpan(message, depth: _activeSpans.Count, this.EndSpan);
            _activeSpans.Push(span);
            return span;
        }

        public void ReplaceOutput(ITraceOutput newOutput)
        {
            _output = newOutput;
        }

        public void SetLevel(TraceLevel newLevel)
        {
            Level = newLevel;
        }

        public TraceLevel Level { get; private set; }

        private void EndSpan(TraceSpan span)
        {
            //TODO: LIFO won't work with async
            if (_activeSpans.Count == 0 || _activeSpans.Peek() != span)
            {
                throw new InvalidOperationException("RealTrace: trace span mismatch");
            }

            _activeSpans.Pop(); 

            if (span.HasLogs || span.HasResult || span.Error != null)
            {
                var record = new TraceRecord(
                    TraceLevel.Span, 
                    message: span.Message,
                    context: GetContextValues(span), 
                    spanDepth: _activeSpans.Count,
                    spanType: span.IsFailure ? TraceRecordSpanType.FinishFailure : TraceRecordSpanType.FinishSuccess);
                _output.WriteRecord(ref record);
            }
        }

        private IReadOnlyList<string> GetContextValues(Func<ITraceContextBuilder, ITraceContextBuilder>? buildContext)
        {
            if (buildContext != null)
            {
                var builder = new ContextBuilder();
                buildContext(builder);
                return builder.GetValues();
            }

            return Array.Empty<string>();
        }

        private IReadOnlyList<string> GetContextValues(TraceSpan finishedSpan)
        {
            if (finishedSpan.Error != null)
            {
                return new[] { finishedSpan.Error.Message };
            }
            else if (finishedSpan.HasResult)
            {
                return new[] { finishedSpan.Result?.ToString() ?? "null" };
            }

            return Array.Empty<string>();
        }

        public static RealTrace SingleInstance { get; } = new RealTrace(new NoopTraceOutput(), TraceLevel.Quiet);

        public static bool Enabled { get; private set; } = false;

        public static void InitializeOutput(ITraceOutput output, TraceLevel level)
        {
            SingleInstance.ReplaceOutput(output);
            SingleInstance.SetLevel(level);
            Enabled = true;
        }

        private class TraceSpan : ITraceSpan
        {
            private readonly Action<TraceSpan> _endSpan;
            private bool _disposed = false;

            public TraceSpan(string message, int depth, Action<TraceSpan> endSpan)
            {
                _endSpan = endSpan;
                Message = message;
                Depth = depth;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _endSpan(this);
                    _disposed = true;
                }
            }

            public T ResultValue<T>(T value)
            {
                Result = value;
                HasResult = true;
                return value;
            }

            public void Failure(Exception? error)
            {
                IsFailure = true;
                Error = error;
            }

            public void SetHasLogs()
            {
                HasLogs = true;
            }

            public string Message { get; }
            public int Depth { get; }
            public bool HasLogs { get; private set; }
            public bool HasResult { get; private set; }
            public object? Result { get; private set; }
            public bool IsFailure { get; private set; }
            public Exception? Error { get; private set; }
        }

        private class ContextBuilder : ITraceContextBuilder
        {
            private readonly List<string> _values = new List<string>();

            ITraceContextBuilder ITraceContextBuilder.Choice<TIn, TOut>(Choice<TIn, TOut> choice)
            {
                Add("choice", choice);
                return this;
            }

            ITraceContextBuilder ITraceContextBuilder.ChoiceMatch<TIn, TOut>(ChoiceMatch<TIn, TOut> match)
            {
                Add("match", match);
                return this;
            }

            ITraceContextBuilder ITraceContextBuilder.ChoiceRefStateMatch<TIn, TOut>(IChoiceRefStateMatch<TIn, TOut> match)
            {
                Add("match", match);
                return this;
            }

            ITraceContextBuilder ITraceContextBuilder.Input<TIn>(IInputContext<TIn> input)
            {
                Add("input", input);
                return this;
            }

            ITraceContextBuilder ITraceContextBuilder.Product<TOut>(OptionalProduct<TOut> product)
            {
                Add("product", product);
                return this;
            }

            ITraceContextBuilder ITraceContextBuilder.Product(object? product)
            {
                Add("product", product);
                return this;
            }

            ITraceContextBuilder ITraceContextBuilder.Result<T>(T value)
            {
                Add("result", value, forceIncludeName: true);
                return this;
            }

            ITraceContextBuilder ITraceContextBuilder.Rule<TIn, TOut>(Rule<TIn, TOut> rule)
            {
                Add("rule", rule);
                return this;
            }

            ITraceContextBuilder ITraceContextBuilder.RuleMatch<TIn, TOut>(RuleMatch<TIn, TOut> match)
            {
                Add("match", match);
                return this;
            }

            ITraceContextBuilder ITraceContextBuilder.RuleRefStateMatch<TIn, TOut>(IRuleRefStateMatch<TIn, TOut> match)
            {
                Add("match", match);
                return this;
            }

            ITraceContextBuilder ITraceContextBuilder.State<TIn>(IState<TIn> state)
            {
                Add("state", state);
                return this;
            }

            ITraceContextBuilder ITraceContextBuilder.StateMatch<TIn>(IStateMatch<TIn> match)
            {
                Add("match", match);
                return this;
            }

            ITraceContextBuilder ITraceContextBuilder.Diagnostic(Diagnostic diagnostic)
            {
                Add("diagnostic", diagnostic);
                return this;
            }


            public ITraceContextBuilder Add(string name, object? value, bool forceIncludeName = false)
            {
                var valueText = $"{(forceIncludeName ? name + "=" : string.Empty)}{value?.ToString() ?? $"{name}=[null]"}";
                _values.Add(valueText);
                return this;
            }

            public IReadOnlyList<string> GetValues() => _values;
        }
    }
}
