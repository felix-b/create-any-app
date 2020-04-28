using System;

namespace LLang.Abstractions
{
    public class BacktrackLabel<TIn>
    {
        public BacktrackLabel(Marker<TIn> marker, BacktrackLabelDescription<TIn> description)
        {
            Marker = marker;
            Description = description;
        }

        public BacktrackLabelDescription<TIn> Description { get; }
        public Marker<TIn> Marker { get; }
    }

    public class BacktrackLabelDescription<TIn>
    {
        public BacktrackLabelDescription(DiagnosticDescription<TIn> diagnostic)
        {
            Diagnostic = diagnostic;
        }
        public BacktrackLabelDescription(string code, Func<Diagnostic<TIn>, string> formatter)
            : this(new DiagnosticDescription<TIn>(code, DiagnosticLevel.Error, formatter))
        {
        }

        public DiagnosticDescription<TIn> Diagnostic { get; }

        public static readonly BacktrackLabelDescription<TIn> Default = new BacktrackLabelDescription<TIn>(
            code: "UNK000",
            formatter: diag => $"Error near '{diag.Input}'");
    }
}
