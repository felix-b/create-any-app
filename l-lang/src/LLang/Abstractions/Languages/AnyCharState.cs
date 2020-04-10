using System;

namespace LLang.Abstractions.Languages
{
    public class AnyCharState : SimpleState<char>
    {
        public AnyCharState(Quantifier? quantifier = null) 
            : base("*", context => true, quantifier)
        {
        }
    }
}