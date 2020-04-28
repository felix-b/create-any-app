using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LLang.Abstractions
{
    public class DiagnosticList<TIn> : IReadOnlyDiagnosticList<TIn>
    {
        private readonly List<Diagnostic<TIn>> _diagnostics = new List<Diagnostic<TIn>>();
        private readonly List<BacktrackLabel<TIn>?> _backtrackLabels = new List<BacktrackLabel<TIn>?>();
        private BacktrackLabel<TIn>? _furthestBacktrackLabel = null;

        IEnumerator<Diagnostic<TIn>> IEnumerable<Diagnostic<TIn>>.GetEnumerator()
        {
            return _diagnostics.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _diagnostics.GetEnumerator();
        }

        Diagnostic<TIn> IReadOnlyList<Diagnostic<TIn>>.this[int index] => _diagnostics[index];

        public void AddDiagnostic(Diagnostic<TIn> diagnostic)
        {
            _diagnostics.Add(diagnostic);

            if (diagnostic.Description.Level == DiagnosticLevel.Error)
            {
                HasErrors = true;
            }
        }

        public void AddBacktrackLabel(BacktrackLabel<TIn> label)
        {
            _backtrackLabels.Add(label);

            if (_furthestBacktrackLabel == null || label.Marker > _furthestBacktrackLabel.Marker)
            {
                _furthestBacktrackLabel = label;
            }
        }

        public void ClearBacktrackLabels(Marker<TIn> untilMarker)
        {
            for (int i = 0 ; i < _backtrackLabels.Count ; i++)
            {
                if (_backtrackLabels[i]?.Marker < untilMarker)
                {
                    _backtrackLabels[i] = null;
                }
            }

            if (_furthestBacktrackLabel?.Marker < untilMarker)
            {
                _furthestBacktrackLabel = null;
            }
        }

        public void CheckForFailures(IInputContext<TIn> context)
        {
            if (_furthestBacktrackLabel != null)
            {
                var input = context.GetSlice(_furthestBacktrackLabel.Marker, _furthestBacktrackLabel.Marker + 1).Span[0]; 
                var failureDiagnostic = new Diagnostic<TIn>(
                    input,
                    _furthestBacktrackLabel.Marker,
                    _furthestBacktrackLabel.Description.Diagnostic);

                _diagnostics.Add(failureDiagnostic);
                _furthestBacktrackLabel = null;
                _backtrackLabels.Clear();

                context.Trace.Diagnostic(failureDiagnostic);
            }
        }

        public bool HasErrors { get; private set; }
        public int Count => _diagnostics.Count;
        public Diagnostic<TIn> this[int index] => _diagnostics[index];
        public IReadOnlyList<BacktrackLabel<TIn>?> BacktrackLabels => _backtrackLabels;
        public BacktrackLabel<TIn>? FurthestBacktrackLabel => _furthestBacktrackLabel;
    }

    public interface IReadOnlyDiagnosticList<TIn> : IReadOnlyList<Diagnostic<TIn>>
    {
        void CheckForFailures(IInputContext<TIn> context);
        bool HasErrors { get; }
        IReadOnlyList<BacktrackLabel<TIn>?> BacktrackLabels { get; }
        BacktrackLabel<TIn>? FurthestBacktrackLabel { get; }
    }
}
