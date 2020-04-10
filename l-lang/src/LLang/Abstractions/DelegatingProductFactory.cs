using System;

namespace LLang.Abstractions
{
    public class DelegatingProductFactory<TIn, TOut> : IProductFactory<TIn, TOut>
    {
        private readonly Func<RuleMatch<TIn, TOut>, IInputContext<TIn>, TOut> _delegate;

        public DelegatingProductFactory(Func<RuleMatch<TIn, TOut>, TOut> @delegate)
            : this((match, getSlice) => @delegate(match))
        {
        }

        public DelegatingProductFactory(Func<RuleMatch<TIn, TOut>, IInputContext<TIn>, TOut> @delegate)
        {
            _delegate = @delegate;
        }

        public TOut Create(RuleMatch<TIn, TOut> match, IInputContext<TIn> context)
        {
            return _delegate(match, context);
        }

        public static implicit operator DelegatingProductFactory<TIn, TOut>(Func<RuleMatch<TIn, TOut>, TOut> @delegate)
        {
            return new DelegatingProductFactory<TIn, TOut>(@delegate);
        }

        public static implicit operator DelegatingProductFactory<TIn, TOut>(Func<RuleMatch<TIn, TOut>, IInputContext<TIn>, TOut> @delegate)
        {
            return new DelegatingProductFactory<TIn, TOut>(@delegate);
        }
    }
}
