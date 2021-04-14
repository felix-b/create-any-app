namespace LLang.Abstractions
{
    public static class Analysis
    {
        public static OptionalProduct<TOut> RunOnce<TIn, TOut>(Grammar<TIn, TOut> grammar, IInputReader<TIn> input)
            where TOut : class
        {
            if (!input.HasInput && !input.ReadNextInput())
            {
                return OptionalProduct.WithoutValue<TOut>();
            }

            var match = grammar.TryMatchStart(input, grammar.FailureDescription);
            if (match == null)
            {
                return OptionalProduct.WithoutValue<TOut>();
            }

            while (input.ReadNextInput())
            {
                if (!match.Next(input))
                {
                    break;
                }
            } 

            if (!match.ValidateMatch(input))
            {
                return OptionalProduct.WithoutValue<TOut>();
            }

            if (match.MatchedRule != null)
            {
                return match.MatchedRule.Product;
            }

            return OptionalProduct.WithoutValue<TOut>();
        }
    }
}
