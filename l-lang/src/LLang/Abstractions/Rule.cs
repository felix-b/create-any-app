using System;
using System.Collections.Generic;
using System.Linq;
using LLang.Tracing;

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

        [Traced]
        public bool MatchAhead(IInputContext<TIn> context)
        {
            return States[0].MatchAhead(context);
        }

        public RuleMatch<TIn, TOut>? TryMatchStart(IInputContext<TIn> context)
        {
            return RuleMatch<TIn, TOut>.TryMatchStart(this, context);
        }

        public override string ToString()
        {
            return $"rule[{Id}]";
        }

        public string Id { get; }
        public List<IState<TIn>> States { get; }
        public IProductFactory<TIn, TOut> ProductFactory { get; }
    }
}
