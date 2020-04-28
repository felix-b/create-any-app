namespace LLang.Abstractions
{
    public interface IState<TIn>
    {
        bool MatchAhead(IInputContext<TIn> context);
        IStateMatch<TIn> CreateMatch(IInputContext<TIn> context, bool initiallyMatched = false);
        string Id { get; }
        Quantifier Quantifier { get; }
        BacktrackLabelDescription<TIn> FailureDescription { get; }
    }
}
