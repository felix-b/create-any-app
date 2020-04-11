using System;
using System.Collections.Generic;
using System.Linq;

namespace LLang.Abstractions
{
    public class ChoiceRefState<TIn, TOut> : IState<TIn>
    {
        public ChoiceRefState(string id, Choice<TIn, TOut> grammarRef, Quantifier? quantifier)
        {
            Id = id;
            GrammarRef = grammarRef;
            Quantifier = quantifier ?? Quantifier.Once;
        }

        public bool MatchAhead(IInputContext<TIn> context)
        {
            return GrammarRef.MatchAhead(context);
        }

        public IStateMatch<TIn> CreateMatch(IInputContext<TIn> context, bool initiallyMatched)
        {
            return new StateMatch(this, GrammarRef, context, initiallyMatched);
        }

        public string Id { get; }
        public Choice<TIn, TOut> GrammarRef { get; }
        public Quantifier Quantifier { get; }

        private class StateMatch : IMatch<TIn>, IStateMatch<TIn>, IChoiceRefStateMatch<TIn, TOut> 
        {
            private readonly List<ChoiceMatch<TIn, TOut>> _grammarMatches;

            public StateMatch(
                ChoiceRefState<TIn, TOut> state, 
                Choice<TIn, TOut> grammarRef,  
                IInputContext<TIn> context,
                bool initiallyMatched)
            {
                State = state;
                StartMarker = context.Mark();
                TimesMatched = 0;
                GrammarRef = grammarRef;
                Input = context.Input;

                _grammarMatches = new List<ChoiceMatch<TIn, TOut>>();

                if (initiallyMatched)
                {
                    _grammarMatches.Add(
                        grammarRef.TryMatchStart(GetReaderFromContext(context)) ?? throw new Exception("Choice cannot be matched")
                    );
                }
            }

            public bool Next(IInputContext<TIn> context)
            {
                if (_grammarMatches.Count == 0)
                {
                    var firstTimeMatch = GrammarRef.TryMatchStart(GetReaderFromContext(context));
                    if (firstTimeMatch != null)
                    {
                        _grammarMatches.Add(firstTimeMatch);
                        return true;
                    }
                    return false;
                }

                var currentGrammarMatch = _grammarMatches[^1];

                if (currentGrammarMatch.Next(context))
                {
                    return true;
                }
                if (!currentGrammarMatch.ValidateMatch(context))
                {
                    return false;
                }
                
                TimesMatched++;
                EndMarker = currentGrammarMatch.EndMarker;

                if (State.Quantifier.Allows(TimesMatched + 1))
                {
                    var nextGrammarMatch = GrammarRef.TryMatchStart(GetReaderFromContext(context));
                    if (nextGrammarMatch != null)
                    {
                        _grammarMatches.Add(nextGrammarMatch);
                        return true;
                    }
                }

                return false;
            }

            public bool ValidateMatch(IInputContext<TIn> context) 
            {
                if (_grammarMatches.Count > 0)
                {
                    var lastGrammarMatch = _grammarMatches[^1];
                    EndMarker = lastGrammarMatch.EndMarker;
                    return lastGrammarMatch.ValidateMatch(context);
                }

                return State.Quantifier.IsMetBy(0);
            }

            public TProduct FindSingleRuleProductOrThrow<TProduct>() 
                where TProduct : class, TOut
            {
                var rule = GrammarMatches.SingleOrDefault().MatchedRule;
                return rule != null && rule.Product.HasValue
                    ? (rule.Product.Value as TProduct 
                        ?? throw new Exception($"GrammarRefState[{State.Id}]: single rule product is not {typeof(TProduct).Name}"))
                    : throw new Exception($"GrammarRefState[{State.Id}]: cannot find single rule product");
            }

            public IState<TIn> State { get; }
            public Choice<TIn, TOut> GrammarRef { get; }
            public IReadOnlyList<ChoiceMatch<TIn, TOut>> GrammarMatches => _grammarMatches;
            public int TimesMatched { get; private set; }
            public Marker<TIn> StartMarker { get; }
            public Marker<TIn> EndMarker { get; private set; }
            public TIn Input { get; }

            private IInputReader<TIn> GetReaderFromContext(IInputContext<TIn> context)
            {
                return (context as IInputReader<TIn>) ?? throw new Exception("IInputContext was expected to also be IInputReader");
            }
        }
    }
}