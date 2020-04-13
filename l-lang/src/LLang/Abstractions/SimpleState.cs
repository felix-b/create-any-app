using System;

namespace LLang.Abstractions
{
    public class SimpleState<TIn> : IState<TIn>
    {
        private readonly Func<IInputContext<TIn>, bool> _predicate;

        public SimpleState(string id, Func<IInputContext<TIn>, bool> predicate, Quantifier? quantifier = null)
        {
            _predicate = predicate;
            Id = id;
            Quantifier = quantifier ?? Quantifier.Once;
        }

        public bool MatchAhead(IInputContext<TIn> context)
        {
            return _predicate(context);
        }

        public IStateMatch<TIn> CreateMatch(IInputContext<TIn> context, bool initiallyMatched = false)
        {
            return new StateMatch(this, _predicate, context, initiallyMatched);
        }

        public string Id { get; }
        public Quantifier Quantifier { get; }

        private class StateMatch : IMatch<TIn>, IStateMatch<TIn>
        {
            private readonly Func<IInputContext<TIn>, bool> _predicate;

            public StateMatch(
                SimpleState<TIn> state, 
                Func<IInputContext<TIn>, bool> predicate,  
                IInputContext<TIn> context, 
                bool initiallyMatched = false)
            {
                _predicate = predicate;
                State = state;
                TimesMatched = initiallyMatched ? 1 : 0;
                StartMarker = context.Mark();
                EndMarker = StartMarker;
                Input = context.Input;
            }

            public bool Next(IInputContext<TIn> context)
            {
                if (State.Quantifier.Allows(TimesMatched + 1) && _predicate(context))
                {
                    TimesMatched++;
                    return true;
                }

                return false;
            }

            public bool ValidateMatch(IInputContext<TIn> context) 
            {
                EndMarker = context.Mark();
                var valid = State.Quantifier.IsMetBy(TimesMatched);
                
                return valid;
            }

            public IState<TIn> State { get; }
            public Marker<TIn> StartMarker { get; }
            public Marker<TIn> EndMarker { get; private set; }
            public TIn Input { get; }
            public int TimesMatched { get; private set; }
        }
    }
}
