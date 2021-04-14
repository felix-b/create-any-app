using System.Collections.Generic;
using System.Linq;
using LLang.Tracing;

namespace LLang.Abstractions
{
    public interface IChoiceMatch<TIn, TOut> : IMatch<TIn, TOut>
    {
        Choice<TIn, TOut> Choice { get; }
        IRuleMatch<TIn, TOut>? MatchedRule { get; }
        IReadOnlyList<IRuleMatch<TIn, TOut>?> MatchingRules { get; }
        IReadOnlyList<IRuleMatch<TIn, TOut>> MatchedRules { get; }
    }
}
