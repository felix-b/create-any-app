using System;
using System.Collections.Generic;
using System.Linq;
using LLang.Tracing;

namespace LLang.Abstractions
{
    public class Rule<TIn, TOut>
    {
        public Rule(string id, IEnumerable<IState<TIn>> states, Func<IRuleMatch<TIn, TOut>, TOut> createProduct)
            : this(id, states, new DelegatingProductFactory<TIn, TOut>(createProduct))
        {
        }

        public Rule(string id, IEnumerable<IState<TIn>> states, Func<IRuleMatch<TIn, TOut>, IInputContext<TIn>, TOut> createProduct)
            : this(id, states, new DelegatingProductFactory<TIn, TOut>(createProduct))
        {
        }

        public Rule(string id, Func<IRuleMatch<TIn, TOut>, IInputContext<TIn>, TOut> createProduct, params IState<TIn>[] states)
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

        public IRuleMatch<TIn, TOut>? TryMatchStart(IInputContext<TIn> context)
        {
            for (int i = 0 ; i < States.Count ; i++)
            {
                var state = States[i];

                if (state.MatchAhead(context))
                {
                    return new RuleMatch(this, context, skippedStateCount: i);
                }
                if (!state.Quantifier.IsMetBy(0))
                {
                    return null;
                }
            }

            return null;
        }

        public override string ToString()
        {
            return $"rule[{Id}]";
        }

        public string Id { get; }
        public List<IState<TIn>> States { get; }
        public IProductFactory<TIn, TOut> ProductFactory { get; }
        
        private class RuleMatch : IRuleMatch<TIn, TOut>
        {
            private readonly List<IStateMatch<TIn>> _matchedStates;

            public RuleMatch(Rule<TIn, TOut> rule, IInputContext<TIn> context, int skippedStateCount)
            {
                Rule = rule;
                StartMarker = context.Mark();
                EndMarker = StartMarker;

                _matchedStates = new List<IStateMatch<TIn>>(capacity: rule.States.Count);
                
                for (int i = 0 ; i < skippedStateCount ; i++)
                {
                    _matchedStates.Add(Rule.States[i].CreateMatch(context, initiallyMatched: false));
                }
                
                _matchedStates.Add(Rule.States[skippedStateCount].CreateMatch(context, initiallyMatched: true));
            }

            [Traced]
            public bool Next(IInputContext<TIn> context)
            {
                while (true)
                {
                    var result = TryMatchCurrentStateOrAdvanceToNext(context);
                    if (result.HasValue)
                    {
                        if (!result.Value && _matchedStates.Count > 0 && _matchedStates.Count < Rule.States.Count)
                        {
                            var unmatchedState = _matchedStates[^1];// Rule.States[_matchedStates.Count];
                            context.EmitBacktrackLabel(new BacktrackLabel<TIn>(context.Mark(), unmatchedState.State.FailureDescription));
                        }
                        return result.Value;
                    }
                }
            }

            [Traced]
            public bool ValidateMatch(IInputContext<TIn> context)
            {
                EndMarker = /*context.Mark();*/ Marker.Max(context.Mark(), StartMarker); //TODO: why does EndMarker < StartMarker ever happen?

                var totalStateCount =  Rule.States.Count;
                var matchedStateCount = _matchedStates.Count;

                if (matchedStateCount > 0 && !_matchedStates[^1].ValidateMatch(context))
                {
                    context.EmitBacktrackLabel(new BacktrackLabel<TIn>(context.Mark(), _matchedStates[^1].State.FailureDescription));
                    return false;
                }

                for (int i = matchedStateCount ; i < totalStateCount; i++)
                {
                    if (!Rule.States[i].Quantifier.IsMetBy(times: 0))
                    {
                        context.EmitBacktrackLabel(new BacktrackLabel<TIn>(context.Mark(), Rule.States[i].FailureDescription));
                        return false;
                    }
                }
                
                context.Trace.Debug("Creating product", x => x.Rule(Rule));
                Product = OptionalProduct.WithValue(Rule.ProductFactory.Create(this, context));

                context.Trace.Success("Created product", x => x.Product(Product.Value).Rule(Rule));
                return true;
            }

            public IStateMatch<TIn> FindStateByIdOrThrow<TState>(string stateId)
                where TState : class, IState<TIn>
            {
                for (int i = 0 ; i < MatchedStates.Count ; i++)
                {
                    if (MatchedStates[i].State is TState state && (stateId == null || stateId == state.Id))
                    {
                        return MatchedStates[i];
                    }
                }
                
                throw new Exception(
                    $"RuleMatch[{Rule.Id}]: cannot find state [{stateId}] of type [{typeof(TState).Name}]");
            }

            public IRuleRefStateMatch<TIn, TOut>? FindRuleById(string ruleId)
            {
                return MatchedStates
                    .OfType<IRuleRefStateMatch<TIn, TOut>>()
                    .FirstOrDefault(m =>
                        m.State is RuleRefState<TIn, TOut> ruleRefState &&
                        ruleRefState.RuleRef.Id == ruleId);
            }

            public IRuleRefStateMatch<TIn, TOut>? FindRuleByStateId(string stateId)
            {
                return MatchedStates
                    .OfType<IRuleRefStateMatch<TIn, TOut>>()
                        .FirstOrDefault(m => m.State.Id == stateId);
            }

            public IChoiceRefStateMatch<TIn, TOut>? FindChoiceByStateId(string stateId)
            {
                return MatchedStates
                    .OfType<IChoiceRefStateMatch<TIn, TOut>>()
                    .FirstOrDefault(m => m.State.Id == stateId);
            }

            public override string ToString()
            {
                var matchedStatesText = string.Join(",", MatchedStates.Select(m => $"{m.TimesMatched}#{m.State.Id}"));
                return $"ruleMatch[{Rule.Id}|{matchedStatesText}]";
            }

            public Rule<TIn, TOut> Rule { get; }
            public Marker<TIn> StartMarker { get; }
            public Marker<TIn> EndMarker { get; private set; }
            public OptionalProduct<TOut> Product { get; private set; }
            public IReadOnlyList<IStateMatch<TIn>> MatchedStates => _matchedStates;

            private bool? TryMatchCurrentStateOrAdvanceToNext(IInputContext<TIn> context)
            {
                var currentStateMatch = _matchedStates[^1];

                if (currentStateMatch.Next(context))
                {
                    return true;
                }

                if (currentStateMatch.ValidateMatch(context) && _matchedStates.Count < Rule.States.Count)
                {
                    _matchedStates.Add(Rule.States[_matchedStates.Count].CreateMatch(context));
                    return null;
                }

                return false;
            }
        }

        private class RecoveringRuleMatch : RecoveringMatch<TIn, TOut>, IRuleMatch<TIn, TOut>
        {
            private readonly IRuleMatch<TIn, TOut> _innerMatch;

            private RecoveringRuleMatch(
                Rule<TIn, TOut> mainRule, 
                Rule<TIn, TOut> recoveryRule,
                IRuleMatch<TIn, TOut> innerMatch)
                : base(innerMatch, recoveryRule)
            {
                _innerMatch = innerMatch;
            }
            
            public IStateMatch<TIn> FindStateByIdOrThrow<TState>(string stateId) where TState : class, IState<TIn>
            {
                return _innerMatch.FindStateByIdOrThrow<TState>(stateId);
            }

            public IRuleRefStateMatch<TIn, TOut>? FindRuleById(string ruleId)
            {
                return _innerMatch.FindRuleById(ruleId);
            }

            public IRuleRefStateMatch<TIn, TOut>? FindRuleByStateId(string stateId)
            {
                return _innerMatch.FindRuleByStateId(stateId);
            }

            public IChoiceRefStateMatch<TIn, TOut>? FindChoiceByStateId(string stateId)
            {
                return _innerMatch.FindChoiceByStateId(stateId);
            }

            public Rule<TIn, TOut> Rule => _innerMatch.Rule;
            
            public IReadOnlyList<IStateMatch<TIn>> MatchedStates => _innerMatch.MatchedStates;

            public static RecoveringRuleMatch Create(
                Rule<TIn, TOut> mainRule,
                Rule<TIn, TOut> recoveryRule,
                IInputContext<TIn> context,
                int skippedStateCount)
            {
                return new RecoveringRuleMatch(
                    mainRule, 
                    recoveryRule,
                    new RuleMatch(mainRule, context, skippedStateCount));
            }
        }
    }
}
