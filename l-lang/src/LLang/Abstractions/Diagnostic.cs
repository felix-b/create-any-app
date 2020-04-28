using System;
using System.Collections.Generic;
using System.Linq;

namespace LLang.Abstractions
{
    public abstract class Diagnostic
    {
        protected Diagnostic(DiagnosticDescription description)
        {
            Description = description;
        }

        public DiagnosticDescription Description { get; }
    }

    public class Diagnostic<TIn> : Diagnostic
    {
        public Diagnostic(TIn input, Marker<TIn> marker, DiagnosticDescription description)
            : base(description)
        {
            Input = input;
            Marker = marker;
        }

        public override string ToString()
        {
            return Description.FormatMessage(this);
        }

        public TIn Input { get; }
        public Marker<TIn> Marker { get; }

        // public IStateMatch<TIn> State { get; }
        // public IReadOnlyList<ErrorLabel> Labels { get; }
        // public IReadOnlyList<IState<TIn>> Expected { get; }
    }

    public enum DiagnosticLevel
    {
        Info,
        Hint,
        Warning,
        Error
    }

    public abstract class DiagnosticDescription
    {
        protected DiagnosticDescription(string code, DiagnosticLevel level)
        {
            Code = code;
            Level = level;
        }
        public string Code { get; }
        public DiagnosticLevel Level { get; }
        public abstract string FormatMessage(Diagnostic occurrence);
    }

    public class DiagnosticDescription<TIn> : DiagnosticDescription
    {
        private readonly Func<Diagnostic<TIn>, string> _formatter;

        public DiagnosticDescription(string code, DiagnosticLevel level, Func<Diagnostic<TIn>, string> formatter)
            : base(code, level)
        {
            _formatter = formatter;
        }

        public override string FormatMessage(Diagnostic occurrence)
        {
            return _formatter((Diagnostic<TIn>)occurrence);
        }
    }
}
