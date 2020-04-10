using System;
using System.Collections.Generic;
using System.Linq;

namespace LLang.Abstractions
{
    public class RuleRefState<TIn, TOut> : IState<TIn>
    {
        public RuleRefState(string id, Rule<TIn, TOut> ruleRef, Quantifier? quantifier)
        {
            Id = id;
            RuleRef = ruleRef;
            Quantifier = quantifier ?? Quantifier.Once;
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

        private class StateMatch : IMatch<TIn>, IStateMatch<TIn>, IRuleRefStateMatch<TIn, TOut> 
        {
            private readonly List<RuleMatch<TIn, TOut>> _ruleMatches;

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

                _ruleMatches = new List<RuleMatch<TIn, TOut>>();

                if (initiallyMatched)
                {
                    _ruleMatches.Add(ruleRef.TryMatchStart(context) ?? throw new Exception("Rule did not match"));
                }

                context.Trace.Debug($"RuleRefStateMatch.ctor(im={initiallyMatched})", x => x.Input(context).RuleRefStateMatch(this));
            }

            public bool Next(IInputContext<TIn> context)
            {
                using var traceSpan = context.Trace.Span($"RuleRefStateMatch.Next", x => x.Input(context).RuleRefStateMatch(this));

                if (_ruleMatches.Count == 0)
                {
                    context.Trace.Debug("first-time match");
                    var firstTimeMatch = RuleRef.TryMatchStart(context);
                    if (firstTimeMatch != null)
                    {
                        _ruleMatches.Add(firstTimeMatch);
                        return traceSpan.ResultValue(true);
                    }
                    return traceSpan.ResultValue(false);
                }

                var currentRuleMatch = _ruleMatches[^1];

                if (currentRuleMatch.Next(context))
                {
                    return traceSpan.ResultValue(true);
                }
                if (!currentRuleMatch.ValidateMatch(context))
                {
                    return traceSpan.ResultValue(false);
                }
                
                TimesMatched++;
                EndMarker = currentRuleMatch.EndMarker;

                if (State.Quantifier.Allows(TimesMatched + 1))
                {
                    var nextRuleMatch = RuleRef.TryMatchStart(context);
                    if (nextRuleMatch != null)
                    {
                        _ruleMatches.Add(nextRuleMatch);
                        return traceSpan.ResultValue(true);
                    }
                }

                return traceSpan.ResultValue(false);
            }

            public bool ValidateMatch(IInputContext<TIn> context) 
            {
                using var traceSpan = context.Trace.Span($"RuleRefStateMatch.ValidateMatch", x => x.Input(context).RuleRefStateMatch(this));
                context.Trace.Debug($"rule matched {_ruleMatches.Count} times");

                if (_ruleMatches.Count > 0)
                {
                    var lastRuleMatch = _ruleMatches[^1];
                    EndMarker = lastRuleMatch.EndMarker;
                    var result = lastRuleMatch.ValidateMatch(context);
                    return traceSpan.ResultValue(result);
                }                

                var zeroTimesResult = State.Quantifier.IsMetBy(0);
                return traceSpan.ResultValue(zeroTimesResult);
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
            public IReadOnlyList<RuleMatch<TIn, TOut>> RuleMatches => _ruleMatches;
            public int TimesMatched { get; private set; }
            public Marker<TIn> StartMarker { get; }
            public Marker<TIn> EndMarker { get; private set; }
            public TIn Input { get; }
        }
    }
}