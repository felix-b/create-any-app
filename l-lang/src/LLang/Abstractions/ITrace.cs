using System;

namespace LLang.Abstractions
{
    public interface ITrace
    {
        void Debug(string message, Func<ITraceContext, ITraceContext>? context = null);
        void Warning(string message, Func<ITraceContext, ITraceContext>? context = null);
        void Error(string message, Func<ITraceContext, ITraceContext>? context = null);
        ITraceSpan Span(string message, Func<ITraceContext, ITraceContext>? context = null);
    }

    public interface ITraceSpan : IDisposable
    {
        T ResultValue<T>(T value);
    }

    public interface ITraceContext
    {
        ITraceContext Input<TIn>(IInputContext<TIn> input);
        ITraceContext State<TIn>(IState<TIn> state);
        ITraceContext Rule<TIn, TOut>(Rule<TIn, TOut> rule);
        ITraceContext Grammar<TIn, TOut>(Grammar<TIn, TOut> grammar);
        ITraceContext Product<TOut>(OptionalProduct<TOut> product);
        ITraceContext Product(object? product);
        ITraceContext GrammarRefStateMatch<TIn, TOut>(IGrammarRefStateMatch<TIn, TOut> match);
        ITraceContext GrammarMatch<TIn, TOut>(GrammarMatch<TIn, TOut> match);
        ITraceContext RuleRefStateMatch<TIn, TOut>(IRuleRefStateMatch<TIn, TOut> match);
        ITraceContext RuleMatch<TIn, TOut>(RuleMatch<TIn, TOut> match);
        ITraceContext StateMatch<TIn>(IStateMatch<TIn> match);
        ITraceContext Result<T>(T value);
    }
}