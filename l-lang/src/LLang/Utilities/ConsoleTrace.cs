using System;
using System.Collections.Generic;
using System.Text;
using LLang.Abstractions;

namespace LLang.Utilities
{
    public class ConsoleTrace : ITrace
    {
        private readonly Stack<TraceSpan> _activeSpans = new Stack<TraceSpan>(); 

        public ConsoleTrace()
        {
            Console.WriteLine();
        }

        public void Debug(string message, Func<ITraceContext, ITraceContext>? context = null)
        {
            WriteMessage(prefix: "   ", message, context);
        }

        public void Warning(string message, Func<ITraceContext, ITraceContext>? context = null)
        {
            WriteMessage(prefix: "/!\\", message, context);
        }

        public void Error(string message, Func<ITraceContext, ITraceContext>? context = null)
        {
            WriteMessage(prefix: "[X]", message, context);
        }

        public ITraceSpan Span(string message, Func<ITraceContext, ITraceContext>? context = null)
        {
            WriteMessage(prefix: "->>", message, context);

            var span = new TraceSpan(message, this.EndSpan);
            _activeSpans.Push(span);
            return span;
        }

        private void EndSpan(TraceSpan span)
        {
            if (_activeSpans.Count == 0 || _activeSpans.Peek() != span)
            {
                throw new InvalidOperationException("Trace span mismatch");
            }
            _activeSpans.Pop();

            if (span.HasLogs || span.HasResult)
            {
                WriteMessage(
                    prefix: "<<-", 
                    message: span.Message, 
                    buildContext: x => span.HasResult ? x.Result(span.Result) : x);
            }
        }

        private void WriteMessage(string prefix, string message, Func<ITraceContext, ITraceContext>? buildContext)
        {
            for (int i = 0 ; i < _activeSpans.Count ; i++)
            {
                Console.Write(" . ");
            }
            Console.Write(prefix.PadRight(4));
            Console.Write(message);
            
            if (buildContext != null)
            {
                var context = new TraceContext();
                buildContext(context);
                Console.Write(context);
            }

            Console.WriteLine();
            
            if (_activeSpans.TryPeek(out var currentSpan))
            {
                currentSpan.SetHasLogs();
            }
        }

        private class TraceSpan : ITraceSpan
        {
            private readonly Action<TraceSpan> _endSpan;

            public TraceSpan(string message, Action<TraceSpan> endSpan)
            {
                _endSpan = endSpan;
                Message = message;
            }

            public void Dispose()
            {
                _endSpan(this);
            }

            public T ResultValue<T>(T value)
            {
                Result = value;
                HasResult = true;
                return value;
            }

            public void SetHasLogs()
            {
                HasLogs = true;
            }

            public string Message { get; }
            public bool HasLogs { get; private set; }
            public bool HasResult { get; private set; }
            public object? Result { get; private set; }
        }

        private class TraceContext : ITraceContext
        {
            private readonly StringBuilder _text = new StringBuilder();

            public ITraceContext Input<TIn>(IInputContext<TIn> input)
            {
                _text.Append(
                    $" [input] pos={input.Mark().Value}" + 
                    (input.IsEndOfInput ? " EOI" : "") +
                    (input.HasInput ? $" val={input.Input}" : " N/A"));
                return this;
            }

            public ITraceContext RuleRefStateMatch<TIn, TOut>(IRuleRefStateMatch<TIn, TOut> match)
            {
                _text.Append(
                    $" [rref/match]" + 
                    $" ruid={match.RuleRef.Id}" +
                    $" stid={match.State.Id}" +
                    $" inp={match.Input}" +
                    $" #m={match.TimesMatched}" +
                    $" #rm={match.RuleMatches.Count}" +
                    $" mrk={match.StartMarker.Value}..{match.EndMarker.Value}");
                return this;
            }

            public ITraceContext GrammarRefStateMatch<TIn, TOut>(IGrammarRefStateMatch<TIn, TOut> match)
            {
                _text.Append(
                    $" [gref/match]" + 
                    $" grid={match.GrammarRef.Id}" +
                    $" stid={match.State.Id}" +
                    $" inp={match.Input}" +
                    $" #m={match.TimesMatched}" +
                    $" #gm={match.GrammarMatches.Count}" +
                    $" mrk={match.StartMarker.Value}..{match.EndMarker.Value}");
                return this;
            }

            public ITraceContext GrammarMatch<TIn, TOut>(GrammarMatch<TIn, TOut> match)
            {
                _text.Append(
                    $" [g/match]" + 
                    $" grid={match.Grammar.Id}" +
                    $" #m/ing={match.MatchingRules.Count}" +
                    $" #m/ed={match.MatchedRules.Count}" +
                    $" mrk={match.StartMarker.Value}..{match.EndMarker.Value}");
                return this;
            }

            public ITraceContext RuleMatch<TIn, TOut>(RuleMatch<TIn, TOut> match)
            {
                _text.Append(
                    $" [r/match]" + 
                    $" ruid={match.Rule.Id}" +
                    $" #ms={match.MatchedStates.Count}" +
                    $" mrk={match.StartMarker.Value}..{match.EndMarker.Value}" +
                    $" p={(match.Product.HasValue ? match.Product.Value?.ToString() : "n/a")}");
                return this;
            }

            public ITraceContext StateMatch<TIn>(IStateMatch<TIn> match)
            {
                _text.Append(
                    $" [s/match]" + 
                    $" stid={match.State.Id}" +
                    $" inp={match.Input}" +
                    $" #m={match.TimesMatched}" +
                    $" mrk={match.StartMarker.Value}..{match.EndMarker.Value}");
                return this;
            }

            public ITraceContext Product<TOut>(OptionalProduct<TOut> product)
            {
                _text.Append($" [prod] {(product.HasValue ? product.Value?.ToString() : "n/a")}");
                return this;
            }

            public ITraceContext Product(object? product)
            {
                _text.Append($" [prod] {product?.ToString() ?? "n/a"}");
                return this;
            }

            public ITraceContext Rule<TIn, TOut>(Rule<TIn, TOut> rule)
            {
                _text.Append($" [rule] id={rule.Id}");
                return this;
            }

            public ITraceContext Grammar<TIn, TOut>(Grammar<TIn, TOut> grammar)
            {
                _text.Append($" [grammar] id={grammar.Id}");
                return this;
            }

            public ITraceContext State<TIn>(IState<TIn> state)
            {
                _text.Append($" [state] id={state.Id}");
                return this;
            }

            public ITraceContext Result<T>(T value)
            {
                _text.Append($" [result] {value}");
                return this;
            }

            public override String ToString()
            {
                return _text.ToString();
            }
        }
    }
}