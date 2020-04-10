using System;
using System.Collections.Generic;
using System.Linq;

namespace LLang.Abstractions
{
    public class Rule<TIn, TOut>
    {
        public Rule(string id, IEnumerable<IState<TIn>> states, Func<RuleMatch<TIn, TOut>, TOut> createProduct)
            : this(id, states, new DelegatingProductFactory<TIn, TOut>(createProduct))
        {
        }

        public Rule(string id, IEnumerable<IState<TIn>> states, Func<RuleMatch<TIn, TOut>, IInputContext<TIn>, TOut> createProduct)
            : this(id, states, new DelegatingProductFactory<TIn, TOut>(createProduct))
        {
        }

        public Rule(string id, Func<RuleMatch<TIn, TOut>, IInputContext<TIn>, TOut> createProduct, params IState<TIn>[] states)
            : this(id, states, new DelegatingProductFactory<TIn, TOut>(createProduct))
        {
        }

        public Rule(string id, IEnumerable<IState<TIn>> states, IProductFactory<TIn, TOut> productFactory)
        {
            Id = id;
            States = states.ToList();
            ProductFactory = productFactory;
        }

        public bool MatchAhead(IInputContext<TIn> context)
        {
            using var traceSpan = context.Trace.Span("Rule.MatchAhead", x => x.Rule(this).Input(context));
            
            var result = States[0].MatchAhead(context);

            return traceSpan.ResultValue(result);
        }

        public RuleMatch<TIn, TOut>? TryMatchStart(IInputContext<TIn> context)
        {
            return RuleMatch<TIn, TOut>.TryMatchStart(this, context);
        }

        public string Id { get; }
        public List<IState<TIn>> States { get; }
        public IProductFactory<TIn, TOut> ProductFactory { get; }
    }
}
