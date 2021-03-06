using System;
using System.Collections.Generic;
using System.Linq;
using LLang.Tracing;

namespace LLang.Abstractions.Languages
{
    public class TokenReader : IInputReader<Token>
    {
        private readonly Token[] _tokens;
        private int _position = -1;

        public TokenReader(ITrace trace, IEnumerable<Token> tokens)
        {
            _tokens = tokens.ToArray();
            Trace = trace;
        }

        public Marker<Token> Mark()
        {
            return new Marker<Token>(_position);
        }

        public bool ReadNextInput()
        {
            if (_position < _tokens.Length)
            {
                _position++;
            }
            return _position < _tokens.Length;
        }

        public void ResetTo(Marker<Token> marker)
        {
            _position = marker.Value;
        }

        public ReadOnlyMemory<Token> GetSlice(
            Marker<Token> start, 
            Marker<Token> end, 
            int startOffset = 0, 
            int endOffset = 0)
        {
            var startPosition = start.Value + startOffset;
            var endPosition = end.Value + endOffset;

            return IsValidPosition(startPosition) && IsValidPosition(endPosition)
                ? new ReadOnlyMemory<Token>(_tokens, start: startPosition, length: endPosition - startPosition)
                : ReadOnlyMemory<Token>.Empty;
        }

        public override string ToString()
        {
            var input = _position switch {
                int p when p < 0 => "BOI",
                int p when p >= _tokens.Length => "EOI",
                _ => _tokens[_position].ToString()
            };
            return $"input[{_position}:{input}]";
        }

        public bool IsEndOfInput => _position >= _tokens.Length;

        public bool HasInput => _position >= 0 && _position < _tokens.Length;

        public Token Input => HasInput ? _tokens[_position] : throw new InvalidOperationException("No input available");

        public ITrace Trace { get; }

        private bool IsValidPosition(int value) => value >= 0 && value <= _tokens.Length;
    }
}
