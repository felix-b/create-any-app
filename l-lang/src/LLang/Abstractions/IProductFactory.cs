using System;

namespace LLang.Abstractions
{
    public interface IProductFactory<TIn, TOut>
    {
        TOut Create(IRuleMatch<TIn, TOut> match, IInputContext<TIn> context);
    }

    public interface IProductOfFactory
    {
    }

    public interface IProductOfFactory<TFactory> : IProductOfFactory
    {
    }

    public delegate TOut CreateRuleProduct<TIn, TOut>(IRuleMatch<TIn, TOut> match, IInputContext<TIn> context);
}
