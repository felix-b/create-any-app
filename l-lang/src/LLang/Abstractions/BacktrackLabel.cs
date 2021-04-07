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

        public override string ToString()
        {
            return $"label[{this.Description.Diagnostic.Code} @ {Marker.Value}]";
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
            code: "LL000",
            formatter: diag => $"Error near '{diag.Input}'");
    }
}
