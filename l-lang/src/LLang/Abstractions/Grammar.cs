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

        public Grammar(string id, IEnumerable<Rule<TIn, TOut>> rules)
            : base(id, rules)
        {
        }
    }
}
