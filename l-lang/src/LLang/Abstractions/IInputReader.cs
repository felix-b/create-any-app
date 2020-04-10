namespace LLang.Abstractions
{
    public interface IInputReader<TIn> : IInputContext<TIn>
    {
        bool ReadNextInput();
        void ResetTo(Marker<TIn> position);
    }
}
