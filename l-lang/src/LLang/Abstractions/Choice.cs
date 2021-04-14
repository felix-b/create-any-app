using System;
using System.Collections.Generic;
using System.Linq;
using LLang.Tracing;

namespace LLang.Abstractions
{
    public class Choice<TIn, TOut>
    {
        public Choice(params Rule<TIn, TOut>[] rules)
            : this(id: string.Empty, rules.AsEnumerable())
        {
        }

        public Choice(string id, params Rule<TIn, TOut>[] rules)
            : this(id, rules.AsEnumerable())
        {
        }

        public Choice(string id, IEnumerable<Rule<TIn, TOut>> rules, BacktrackLabelDescription<TIn>? failureDescription = null)
        {
            Id = id;
            Rules = rules.ToList();
            FailureDescription = failureDescription ?? new BacktrackLabelDescription<TIn>($"LL004[choice={id}]", FormatDefaultFailure);
        }

        [Traced]
        public bool MatchAhead(IInputContext<TIn> context)
        {
            for (int i = 0 ; i < Rules.Count ; i++) 
            {
                if (Rules[i].MatchAhead(context))
                {
                    return true;
                }
            }
            return false;
        }

        [Traced]
        public IChoiceMatch<TIn, TOut>? TryMatchStart(IInputReader<TIn> input, BacktrackLabelDescription<TIn> failureDescription)
        {
            List<IRuleMatch<TIn, TOut>?>? matchingRules = null;

            for (int i = 0 ; i < Rules.Count ; i++)
            {
                var match = Rules[i].TryMatchStart(input);
                if (match != null)
                {
                    if (matchingRules == null)
                    {
                        matchingRules = new List<IRuleMatch<TIn, TOut>?>(capacity: Rules.Count);
                    }
                    matchingRules.Add(match);
                }
            }

            if (matchingRules != null)
            {
                return new ChoiceMatch(this, input, matchingRules, failureDescription);
            }

            input.EmitBacktrackLabel(new BacktrackLabel<TIn>(input.Mark(), failureDescription));
            return null;
        }

        public override string ToString()
        {
            return $"choice[{Id}]";
        }   

        public string Id { get; }
        public List<Rule<TIn, TOut>> Rules { get; }
        public BacktrackLabelDescription<TIn> FailureDescription { get; }

        private string FormatDefaultFailure(Diagnostic<TIn> diagnostic)
        {
            var choicesText = Rules.Count switch
            {
                1 => Rules[0].Id,
                2 => $"{Rules[0].Id} or {Rules[1].Id}",
                3 => $"{Rules[0].Id}, {Rules[1].Id}, or {Rules[2].Id}",
                _ => $"{string.Join(", ", Rules.Take(3).Select(r => r.Id))}, ..."
            };

            var thisText = string.IsNullOrEmpty(Id) ? choicesText : Id;
            return $"Expected {thisText}, but found: {diagnostic.Input}";
        }
        
        private class ChoiceMatch : IChoiceMatch<TIn, TOut>
        {
            private readonly IInputReader<TIn> _reader;
            private readonly List<IRuleMatch<TIn, TOut>?> _matchingRules;
            private readonly List<IRuleMatch<TIn, TOut>> _matchedRules;
            private readonly BacktrackLabelDescription<TIn> _failureDescription;

            public ChoiceMatch(
                Choice<TIn, TOut> choice, 
                IInputReader<TIn> reader,
                List<IRuleMatch<TIn, TOut>?> matchingRules,
                BacktrackLabelDescription<TIn> failureDescription)
            {
                _reader = reader;
                _matchingRules = matchingRules;
                _matchedRules = new List<IRuleMatch<TIn, TOut>>(capacity: choice.Rules.Count);
                _failureDescription = failureDescription;

                Choice = choice;
                StartMarker = reader.Mark();
                EndMarker = StartMarker;
            }

            [Traced]
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

                if (!anyRuleMatched)
                {
                    if (_matchedRules.Count > 0)
                    {
                        MatchedRule = FindBestMatchedRule();
                        RevertInputToMatchedRuleEnd();
                    }
                    else 
                    {
                        context.EmitBacktrackLabel(new BacktrackLabel<TIn>(context.Mark(), _failureDescription));
                    }
                }

                return anyRuleMatched;
            }

            [Traced]
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

            public override string ToString()
            {
                var matchingRulesText = string.Join(",", MatchingRules.Select(m => m?.Rule.Id ?? "#"));
                var matchedRulesText = string.Join(",", MatchedRules.Select(m => m.Rule.Id));
                return $"choiceMatch[{Choice.Id}|{matchingRulesText}|{matchedRulesText}]";
            }

            public Choice<TIn, TOut> Choice { get; }
            public Marker<TIn> StartMarker { get; }
            public Marker<TIn> EndMarker { get; private set; }
            public IRuleMatch<TIn, TOut>? MatchedRule { get; private set; }
            public IReadOnlyList<IRuleMatch<TIn, TOut>?> MatchingRules => _matchingRules;
            public IReadOnlyList<IRuleMatch<TIn, TOut>> MatchedRules => _matchedRules;
            public OptionalProduct<TOut> Product => MatchedRule?.Product ?? OptionalProduct.WithoutValue<TOut>();

            private void RevertInputToMatchedRuleEnd()
            {
                var resetMarker = MatchedRule != null
                    ? MatchedRule.EndMarker
                    : StartMarker;

                EndMarker = resetMarker;
                _reader.ResetTo(resetMarker);
            }

            private IRuleMatch<TIn, TOut>? FindBestMatchedRule()
            {
                var maxLength = -1;
                IRuleMatch<TIn, TOut>? bestRule = null;

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

            public static IChoiceMatch<TIn, TOut>? TryMatchStart(
                Choice<TIn, TOut> choice, 
                IInputReader<TIn> reader,
                BacktrackLabelDescription<TIn> failureDescription)
            {
                List<IRuleMatch<TIn, TOut>?>? matchingRules = null;

                for (int i = 0 ; i < choice.Rules.Count ; i++)
                {
                    var match = choice.Rules[i].TryMatchStart(reader);
                    if (match != null)
                    {
                        if (matchingRules == null)
                        {
                            matchingRules = new List<IRuleMatch<TIn, TOut>?>(capacity: choice.Rules.Count);
                        }
                        matchingRules.Add(match);
                    }
                }

                if (matchingRules != null)
                {
                    return new ChoiceMatch(choice, reader, matchingRules, failureDescription);
                }

                reader.EmitBacktrackLabel(new BacktrackLabel<TIn>(reader.Mark(), failureDescription));
                return null;
            }
        }
        
    }   
}
