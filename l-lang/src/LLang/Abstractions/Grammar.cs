using System;
using System.Collections.Generic;
using System.Linq;

namespace LLang.Abstractions
{
    public class Grammar<TIn, TOut>
    {
        public Grammar(params Rule<TIn, TOut>[] rules)
            : this(id: string.Empty, rules.AsEnumerable())
        {
        }

        public Grammar(string id, params Rule<TIn, TOut>[] rules)
            : this(id, rules.AsEnumerable())
        {
        }

        public Grammar(string id, IEnumerable<Rule<TIn, TOut>> rules)
        {
            Id = id;
            Rules = rules.ToList();
        }

        public bool MatchAhead(IInputContext<TIn> context)
        {
            using var traceSpan = context.Trace.Span("Grammar.MatchAhead", x => x.Grammar(this).Input(context));

            for (int i = 0 ; i < Rules.Count ; i++) 
            {
                context.Trace.Debug($"rule {i}/{Rules.Count}: try to match-ahead", x => x.Rule(Rules[i]));
                
                if (Rules[i].MatchAhead(context))
                {
                    context.Trace.Debug($"RULE MATCH-AHEAD SUCCESS", x => x.Rule(Rules[i]));
                    return traceSpan.ResultValue(true);
                }
            }
            
            context.Trace.Debug($"ALL RULES MATCH-AHEAD FAILED", x => x.Grammar(this).Input(context));
            return traceSpan.ResultValue(false);
        }

        public GrammarMatch<TIn, TOut>? TryMatchStart(IInputReader<TIn> input)
        {
            return GrammarMatch<TIn, TOut>.TryMatchStart(this, input);
        }

        public string Id { get; }
        public List<Rule<TIn, TOut>> Rules { get; }
    }
}
