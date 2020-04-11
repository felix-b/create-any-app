namespace LLang.Abstractions
{
    public static class Analysis
    {
        public static OptionalProduct<TOut> RunOnce<TIn, TOut>(Grammar<TIn, TOut> grammar, IInputReader<TIn> input)
            where TOut : class
        {
            using var traceSpan = input.Trace.Span(
                $"Analysis.RunOnce<{typeof(TIn).Name},{typeof(TOut).Name}>", 
                x => x.Choice(grammar).Input(input));

            if (!input.HasInput && !input.ReadNextInput())
            {
                input.Trace.Warning("Empty input");
                return traceSpan.ResultValue(OptionalProduct.WithoutValue<TOut>());
            }

            var match = grammar.TryMatchStart(input);
            if (match == null)
            {
                input.Trace.Error("Grammar not matched on start");
                return traceSpan.ResultValue(OptionalProduct.WithoutValue<TOut>());
            }

            while (input.ReadNextInput())
            {
                using var nextInputSpan = input.Trace.Span("Next-input", x => x.Input(input));
                
                if (!match.Next(input))
                {
                    nextInputSpan.ResultValue(false);
                    break;
                }
                
                nextInputSpan.ResultValue(true);
            } 

            if (!match.ValidateMatch(input))
            {
                input.Trace.Error("Grammar match validation failed");
                return traceSpan.ResultValue(OptionalProduct.WithoutValue<TOut>());
            }

            if (match.MatchedRule != null)
            {
                input.Trace.Debug("SUCCESS");
                return traceSpan.ResultValue(match.MatchedRule.Product);
            }

            input.Trace.Warning("No matched rule, no product");
            return traceSpan.ResultValue(OptionalProduct.WithoutValue<TOut>());
        }
    }
}
