using System;
using System.Collections.Generic;
using System.Linq;

namespace LLang.Abstractions
{
    public class RuleRefState<TIn, TOut> : IState<TIn>
    {
        public RuleRefState(
            string id, 
            Rule<TIn, TOut> ruleRef, 
            Quantifier? quantifier, 
            BacktrackLabelDescription<TIn>? failureDescription = null)
        {
            Id = id;
            RuleRef = ruleRef;
            Quantifier = quantifier ?? Quantifier.Once;
            FailureDescription = failureDescription ?? new BacktrackLabelDescription<TIn>($"LL003[rule={ruleRef.Id}]", d => $"Expected {RuleRef.Id}, but found: '{d.Input}'");
        }

        public bool MatchAhead(IInputContext<TIn> context)
        {
            return RuleRef.MatchAhead(context);
        }

        public IStateMatch<TIn> CreateMatch(IInputContext<TIn> context, bool initiallyMatched)
        {
            return new StateMatch(this, RuleRef, context, initiallyMatched);
        }

        public string Id { get; }
        public Rule<TIn, TOut> RuleRef { get; }
        public Quantifier Quantifier { get; }
        public BacktrackLabelDescription<TIn> FailureDescription { get; }


        private class StateMatch : IMatch<TIn>, IStateMatch<TIn>, IRuleRefStateMatch<TIn, TOut> 
        {
            private readonly List<IRuleMatch<TIn, TOut>> _ruleMatches;

            public StateMatch(
                RuleRefState<TIn, TOut> state, 
                Rule<TIn, TOut> ruleRef,  
                IInputContext<TIn> context,
                bool initiallyMatched)
            {
                State = state;
                StartMarker = context.Mark();
                EndMarker = StartMarker;
                TimesMatched = 0;
                RuleRef = ruleRef;
                Input = context.Input;

                _ruleMatches = new List<IRuleMatch<TIn, TOut>>();

                if (initiallyMatched)
                {
                    _ruleMatches.Add(ruleRef.TryMatchStart(context) ?? throw new Exception("Rule did not match"));
                }
            }

            public bool Next(IInputContext<TIn> context)
            {
                if (_ruleMatches.Count == 0)
                {
                    var firstTimeMatch = RuleRef.TryMatchStart(context);
                    if (firstTimeMatch != null)
                    {
                        _ruleMatches.Add(firstTimeMatch);
                        return true;
                    }
                    return false;
                }

                var currentRuleMatch = _ruleMatches[^1];

                if (currentRuleMatch.Next(context))
                {
                    return true;
                }
                if (!currentRuleMatch.ValidateMatch(context))
                {
                    return false;
                }
                
                TimesMatched++;
                EndMarker = currentRuleMatch.EndMarker;

                if (State.Quantifier.Allows(TimesMatched + 1))
                {
                    var nextRuleMatch = RuleRef.TryMatchStart(context);
                    if (nextRuleMatch != null)
                    {
                        _ruleMatches.Add(nextRuleMatch);
                        return true;
                    }
                }

                return false;
            }

            public bool ValidateMatch(IInputContext<TIn> context) 
            {
                if (_ruleMatches.Count > 0)
                {
                    var lastRuleMatch = _ruleMatches[^1];
                    EndMarker = lastRuleMatch.EndMarker;
                    return lastRuleMatch.ValidateMatch(context);
                }                

                return State.Quantifier.IsMetBy(0);
            }

            public TProduct FindSingleRuleProductOrThrow<TProduct>() 
                where TProduct : class, TOut
            {
                var optionalProduct = RuleMatches.SingleOrDefault()?.Product;
                return optionalProduct.HasValue && optionalProduct.Value.HasValue
                    ? (optionalProduct.Value.Value as TProduct
                        ?? throw new Exception($"RuleRefState[{State.Id}]: single rule product is not {typeof(TProduct).Name}"))
                    : throw new Exception($"RuleRefState[{State.Id}]: cannot find single rule product");
            }

            public IState<TIn> State { get; }
            public Rule<TIn, TOut> RuleRef { get; }
            public IReadOnlyList<IRuleMatch<TIn, TOut>> RuleMatches => _ruleMatches;
            public int TimesMatched { get; private set; }
            public Marker<TIn> StartMarker { get; }
            public Marker<TIn> EndMarker { get; private set; }
            public TIn Input { get; }
        }
    }
}
