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
            FailureDescription = failureDescription ?? new BacktrackLabelDescription<TIn>("LL004", FormatDefaultFailure);
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
        public ChoiceMatch<TIn, TOut>? TryMatchStart(IInputReader<TIn> input, BacktrackLabelDescription<TIn>? failureDescription = null)
        {
            return ChoiceMatch<TIn, TOut>.TryMatchStart(this, input, failureDescription ?? this.FailureDescription);
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
    }   
}
