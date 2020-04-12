using System.Collections.Generic;

namespace LLang.Abstractions
{
    public class ChoiceMatch<TIn, TOut> : IMatch<TIn>
    {
        private readonly IInputReader<TIn> _reader;
        private readonly List<RuleMatch<TIn, TOut>?> _matchingRules;
        private readonly List<RuleMatch<TIn, TOut>> _matchedRules;

        private ChoiceMatch(
            Choice<TIn, TOut> choice, 
            IInputReader<TIn> reader,
            List<RuleMatch<TIn, TOut>?> matchingRules)
        {
            _reader = reader;
            _matchingRules = matchingRules;
            _matchedRules = new List<RuleMatch<TIn, TOut>>(capacity: choice.Rules.Count);

            Choice = choice;
            StartMarker = reader.Mark();
            EndMarker = StartMarker;
        }


        public bool Next(IInputContext<TIn> context)
        {
            var anyRuleMatched = false;

            for (int i = 0 ; i < _matchingRules.Count ; i++)
            {
                var rule = _matchingRules[i];
                if (rule == null)
                {
                    continue;
                }
                if (rule.Next(context))
                {
                    anyRuleMatched = true;
                }
                else
                {
                    if (rule.ValidateMatch(context))
                    {
                        _matchedRules.Add(rule);
                    }
                    _matchingRules[i] = null;
                }
            }   

            if (!anyRuleMatched && _matchedRules.Count > 0)
            {
                MatchedRule = FindBestMatchedRule();
                RevertInputToMatchedRuleEnd();
            }

            return anyRuleMatched;
        }

        public bool ValidateMatch(IInputContext<TIn> context)
        {
            for (int i = 0 ; i < _matchingRules.Count ; i++)
            {
                var rule = _matchingRules[i];
                if (rule != null && rule.ValidateMatch(context))
                {
                    _matchedRules.Add(rule);
                    _matchingRules[i] = null;
                }
            }

            MatchedRule = FindBestMatchedRule();
            RevertInputToMatchedRuleEnd();

            return MatchedRule != null;
        }

        public Choice<TIn, TOut> Choice { get; }
        public Marker<TIn> StartMarker { get; }
        public Marker<TIn> EndMarker { get; private set; }
        public RuleMatch<TIn, TOut>? MatchedRule { get; private set; }
        public IReadOnlyList<RuleMatch<TIn, TOut>?> MatchingRules => _matchingRules;
        public IReadOnlyList<RuleMatch<TIn, TOut>> MatchedRules => _matchedRules;

        private void RevertInputToMatchedRuleEnd()
        {
            var resetMarker = MatchedRule != null
                ? MatchedRule.EndMarker
                : StartMarker;

            EndMarker = resetMarker;
            _reader.ResetTo(resetMarker);
        }

        private RuleMatch<TIn, TOut>? FindBestMatchedRule()
        {
            var maxLength = -1;
            RuleMatch<TIn, TOut>? bestRule = null;

            for (int i = 0 ; i < _matchedRules.Count ; i++)
            {
                var rule = _matchedRules[i];
                var length = rule.EndMarker - rule.StartMarker;

                if (length > maxLength)
                {
                    maxLength = length;
                    bestRule = rule;
                }
            }

            return bestRule;
        }

        public static ChoiceMatch<TIn, TOut>? TryMatchStart(
            Choice<TIn, TOut> choice, 
            IInputReader<TIn> reader)
        {
            List<RuleMatch<TIn, TOut>?>? matchingRules = null;

            for (int i = 0 ; i < choice.Rules.Count ; i++)
            {
                var match = choice.Rules[i].TryMatchStart(reader);
                if (match != null)
                {
                    if (matchingRules == null)
                    {
                        matchingRules = new List<RuleMatch<TIn, TOut>?>(capacity: choice.Rules.Count);
                    }
                    matchingRules.Add(match);
                }
            }

            return matchingRules != null 
                ? new ChoiceMatch<TIn, TOut>(choice, reader, matchingRules)
                : null;
        }
    }
}
