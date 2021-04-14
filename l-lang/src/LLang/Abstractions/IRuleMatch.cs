using System;
using System.Collections.Generic;
using System.Linq;
using LLang.Tracing;

namespace LLang.Abstractions
{
    public interface IRuleMatch<TIn, TOut> : IMatch<TIn, TOut>
    {
        IStateMatch<TIn> FindStateByIdOrThrow<TState>(string stateId)
            where TState : class, IState<TIn>;
        IRuleRefStateMatch<TIn, TOut>? FindRuleById(string ruleId);
        IRuleRefStateMatch<TIn, TOut>? FindRuleByStateId(string stateId);
        IChoiceRefStateMatch<TIn, TOut>? FindChoiceByStateId(string stateId);
        Rule<TIn, TOut> Rule { get; }
        IReadOnlyList<IStateMatch<TIn>> MatchedStates { get; }
    }
}
