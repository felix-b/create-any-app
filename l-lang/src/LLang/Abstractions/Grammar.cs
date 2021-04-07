using System;
using System.Collections.Generic;
using System.Linq;

namespace LLang.Abstractions
{
    public class Grammar<TIn, TOut> : Choice<TIn, TOut>
    {
        public Grammar(params Rule<TIn, TOut>[] rules)
            : this(id: string.Empty, rules.AsEnumerable())
        {
        }

        public Grammar(string id, params Rule<TIn, TOut>[] rules)
            : this(id, rules.AsEnumerable())
        {
        }

        public Grammar(string id, BacktrackLabelDescription<TIn> failure)
            : base(id, Enumerable.Empty<Rule<TIn, TOut>>(), failure)
        {
        }

        public Grammar(string id, IEnumerable<Rule<TIn, TOut>> rules, BacktrackLabelDescription<TIn>? failure = null)
            : base(id, rules, failure)
        {
        }

        public override string ToString()
        {
            return $"grammar<{typeof(TIn).Name[0]},{typeof(TOut).Name[0]}>{(Id.Length > 0 ? $"[{Id}]" : "")}";
        }
    }
}
