using System;
using System.Collections.Generic;
using System.Linq;

namespace LLang.Abstractions
{
    public class RuleMatch<TIn, TOut> : IMatch<TIn>
    {
        private readonly List<IStateMatch<TIn>> _matchedStates;

        private RuleMatch(Rule<TIn, TOut> rule, IInputContext<TIn> context, int skippedStateCount)
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

        public bool Next(IInputContext<TIn> context)
        {
            using var traceSpan = context.Trace.Span("RuleMatch.Next", x => x.Input(context).RuleMatch(this));

            while (true)
            {
                var result = TryMatchCurrentStateOrAdvanceToNext(context);
                if (result.HasValue)
                {
                    return traceSpan.ResultValue(result.Value);
                }
            }
        }

        public bool ValidateMatch(IInputContext<TIn> context)
        {
            using var traceSpan = context.Trace.Span("RuleMatch.Next", x => x.Input(context).RuleMatch(this));

            EndMarker = context.Mark();

            var totalStateCount =  Rule.States.Count;
            var matchedStateCount = _matchedStates.Count;

            if (matchedStateCount > 0 && !_matchedStates[matchedStateCount - 1].ValidateMatch(context))
            {
                context.Trace.Debug("last matched state failed to validate");
                return traceSpan.ResultValue(false);
            }
            for (int i = matchedStateCount ; i < totalStateCount; i++)
            {
                if (!Rule.States[i].Quantifier.IsMetBy(times: 0))
                {
                    context.Trace.Debug("unreached state to validate");
                    return traceSpan.ResultValue(false);
                }
            }
            
            Product = OptionalProduct.WithValue(Rule.ProductFactory.Create(this, context));
            
            context.Trace.Debug("created rule product", x => x.Product(Product).Rule(Rule));
            return traceSpan.ResultValue(true);
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

        public IGrammarRefStateMatch<TIn, TOut>? FindChoiceByStateId(string stateId)
        {
            return MatchedStates
                .OfType<IGrammarRefStateMatch<TIn, TOut>>()
                .FirstOrDefault(m => m.State.Id == stateId);
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

        public static RuleMatch<TIn, TOut>? TryMatchStart(Rule<TIn, TOut> rule, IInputContext<TIn> context)
        {
            using var traceSpan = context.Trace.Span("RuleMatch.TryMatchStart", x => x.Rule(rule).Input(context));

            for (int i = 0 ; i < rule.States.Count ; i++)
            {
                var state = rule.States[i];

                context.Trace.Debug($"attempting state {i+1}/{rule.States.Count}", x => x.State(state));

                if (state.MatchAhead(context))
                {
                    var newMatch = new RuleMatch<TIn, TOut>(rule, context, skippedStateCount: i);
                    context.Trace.Debug($"RULE MATCH-AHEAD SUCCESS", x => x.RuleMatch(newMatch));
                    return traceSpan.ResultValue(newMatch);
                }
                if (!state.Quantifier.IsMetBy(0))
                {
                    context.Trace.Debug($"RULE MATCH-AHEAD FAILURE", x => x.Rule(rule));
                    return traceSpan.ResultValue<RuleMatch<TIn, TOut>?>(null);
                }
            }
            return null;
        }
    }
}
