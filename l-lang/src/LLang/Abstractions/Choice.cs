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

        public Choice(string id, IEnumerable<Rule<TIn, TOut>> rules)
        {
            Id = id;
            Rules = rules.ToList();
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
        public ChoiceMatch<TIn, TOut>? TryMatchStart(IInputReader<TIn> input)
        {
            return ChoiceMatch<TIn, TOut>.TryMatchStart(this, input);
        }

        public override string ToString()
        {
            return $"choice[{Id}]";
        }   

        public string Id { get; }
        public List<Rule<TIn, TOut>> Rules { get; }
    }
}
