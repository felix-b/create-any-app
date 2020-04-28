using System;
using System.IO;
using LLang.Tracing;
using LLang.Utilities;

namespace LLang.Abstractions.Languages
{
    public class SourceFileReader : IInputReader<char>
    {
        private readonly LexicalDiagnosticList _diagnostics = new LexicalDiagnosticList();
        private readonly string _filePath;
        private readonly string _text;
        private int _position = -1;

        public SourceFileReader(ITrace trace, string filePath, TextReader reader)
        {
            _filePath = filePath;
            _text = reader.ReadToEnd();
            Trace = trace;
        }

        public void EmitDiagnostic(Diagnostic<char> diagnostic)
        {
            _diagnostics.AddDiagnostic(diagnostic);
        }

        public void EmitBacktrackLabel(BacktrackLabel<char> label)
        {
            _diagnostics.AddBacktrackLabel(label);
        }

        public Location GetLocation(Marker<char> marker)
        {
            return new Location(_filePath, 1, marker.Value);
        }

        public ReadOnlyMemory<char> GetSlice(
            Marker<char> start, 
            Marker<char> end, 
            int startOffset = 0, 
            int endOffset = 0)
        {
            var startPosition = start.Value + startOffset;
            var endPosition = end.Value + endOffset;

            return IsValidPosition(startPosition) && IsValidPosition(endPosition)
                ? _text.AsMemory(start: startPosition, length: endPosition - startPosition)
                : ReadOnlyMemory<char>.Empty;
        }

        public Marker<char> Mark() 
        {
            return new Marker<char>(_position);
        }

        public bool ReadNextInput()
        {
            if (_position < _text.Length)
            {
                _position++;
            }
            return _position < _text.Length;
        }

        public void ResetTo(Marker<char> position)
        {
            _position = position.Value;
            _diagnostics.ClearBacktrackLabels(untilMarker: position);
        }

        public void CheckForFailures()
        {
            Diagnostics.CheckForFailures(this);
        }

        public override string ToString()
        {
            var input = _position switch {
                int p when p < 0 => "BOI",
                int p when p >= _text.Length => "EOI",
                _ => _text[_position].EscapeIfControl()
            };
            return $"input[{_position}:{input}]";
        }

        public IReadOnlyDiagnosticList<char> Diagnostics => _diagnostics;
        public bool IsEndOfInput => _position >= _text.Length;
        public bool HasInput => _position >= 0 && _position < _text.Length;
        public char Input => HasInput ? _text[_position] : throw new InvalidOperationException("No input available");
        public ITrace Trace { get; }

        private bool IsValidPosition(int value) => value >= 0 && value <= _text.Length;
    }
}
