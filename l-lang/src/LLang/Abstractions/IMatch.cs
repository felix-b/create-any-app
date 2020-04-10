namespace LLang.Abstractions
{
    public interface IMatch<TIn>
    {
        bool Next(IInputContext<TIn> context);
        bool ValidateMatch(IInputContext<TIn> context);
        Marker<TIn> StartMarker { get; }
        Marker<TIn> EndMarker { get; }
    }
}
