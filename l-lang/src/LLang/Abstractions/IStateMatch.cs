using System.Collections.Generic;

namespace LLang.Abstractions
{
    public interface IStateMatch<TIn> : IMatch<TIn>
    {
        IState<TIn> State { get; }
        TIn Input { get; }
        int TimesMatched { get; }
    }

    public interface IRuleRefStateMatch<TIn, TOut> : IStateMatch<TIn>
    {
        TProduct FindSingleRuleProductOrThrow<TProduct>() where TProduct : class, TOut;
        Rule<TIn, TOut> RuleRef { get; }
        IReadOnlyList<IRuleMatch<TIn, TOut>> RuleMatches { get; }
    }

    public interface IChoiceRefStateMatch<TIn, TOut> : IStateMatch<TIn>
    {
        TProduct FindSingleRuleProductOrThrow<TProduct>() where TProduct : class, TOut;
        Choice<TIn, TOut> GrammarRef { get; }
        IReadOnlyList<IChoiceMatch<TIn, TOut>> GrammarMatches { get; }
    }
}
