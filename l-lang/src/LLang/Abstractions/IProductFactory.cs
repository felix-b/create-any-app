using System;

namespace LLang.Abstractions
{
    public interface IProductFactory<TIn, TOut>
    {
        TOut Create(RuleMatch<TIn, TOut> match, IInputContext<TIn> context);
    }

    public interface IProductOfFactory
    {
    }

    public interface IProductOfFactory<TFactory> : IProductOfFactory
    {
    }

    public delegate TOut CreateRuleProduct<TIn, TOut>(RuleMatch<TIn, TOut> match, IInputContext<TIn> context);
}
