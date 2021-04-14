using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LLang.Abstractions
{
    public class DiagnosticList<TIn> : IReadOnlyDiagnosticList<TIn>
    {
        private readonly IInputContext<TIn> _context;
        private readonly List<Diagnostic<TIn>> _diagnostics = new List<Diagnostic<TIn>>();
        private readonly List<BacktrackLabel<TIn>?> _backtrackLabels = new List<BacktrackLabel<TIn>?>();
        private BacktrackLabel<TIn>? _furthestBacktrackLabel = null;

        public DiagnosticList(IInputContext<TIn> context)
        {
            _context = context;
        }

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

            _context.Trace.Debug("BKTRAK", x => x.BacktrackLabel(label));
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

        public void CheckForFailures()
        {
            if (_furthestBacktrackLabel != null)
            {
                var inputSlice = _context.GetSlice(_furthestBacktrackLabel.Marker, _furthestBacktrackLabel.Marker + 1);
                var input = !inputSlice.IsEmpty ? inputSlice.Span[0] : default; //TODO: why the input slice can ever be empty?
                var failureDiagnostic = new Diagnostic<TIn>(
                    input!, //TODO: get rid of '!'
                    _furthestBacktrackLabel.Marker,
                    _furthestBacktrackLabel.Description.Diagnostic);

                var b = new StringBuilder();
                _backtrackLabels.ForEach(l => {
                    if (l != null)
                    {
                        var d = new Diagnostic<TIn>(
                            input!, //TODO: get rid of '!'
                            l.Marker,
                            l.Description.Diagnostic);

                        b.Append(l.Description.Diagnostic.Code);
                        b.Append(':');
                        b.AppendLine(l.Description.Diagnostic.FormatMessage(d));
                    }
                });
                Console.WriteLine(b);
                
                _diagnostics.Add(failureDiagnostic);
                _furthestBacktrackLabel = null;
                _backtrackLabels.Clear();

                _context.Trace.Diagnostic(failureDiagnostic);
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
        void CheckForFailures();
        bool HasErrors { get; }
        IReadOnlyList<BacktrackLabel<TIn>?> BacktrackLabels { get; }
        BacktrackLabel<TIn>? FurthestBacktrackLabel { get; }
    }
}
