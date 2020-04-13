using System;
using System.Collections.Generic;
using System.Text;
using LLang.Abstractions;

namespace LLang.Tracing
{
    public class ConsoleTraceOutput : ITraceOutput
    {
        private readonly bool _useColors;

        public ConsoleTraceOutput(bool useColors)
        {
            _useColors = useColors;
        }

        public void WriteRecord(ref TraceRecord record)
        {
            var (actualLevel, spanIcon) = record.SpanType switch
            {
                TraceRecordSpanType.Start => (TraceLevel.Span, "->>"),
                TraceRecordSpanType.FinishSuccess => (TraceLevel.Span, "<<-"),
                TraceRecordSpanType.FinishFailure => (TraceLevel.Error, "<<-"),
                _ => (record.Level, null)
            };

            var (color, levelIcon) = actualLevel switch
            {
                TraceLevel.Debug => (ConsoleColor.Gray, string.Empty),
                TraceLevel.Success => (ConsoleColor.Cyan, "(i)"),
                TraceLevel.Warning => (ConsoleColor.Yellow, @"/!\"),
                TraceLevel.Error => (ConsoleColor.Red, "[X]"),
                _ => (ConsoleColor.DarkGray, "???"),
            };

            WriteMessage(ref record, color, spanIcon ?? levelIcon);
        }

        private void WriteMessage(ref TraceRecord record, ConsoleColor color, string icon)
        {
            using var restoreColor = SaveColor();

            SetColor(ConsoleColor.DarkGray);
            for (int i = 0 ; i < record.SpanDepth ; i++)
            {
                Console.Write(" . ");
            }
            
            SetColor(color);
            Console.Write(icon.PadRight(4));
            Console.Write(record.Message);

            if (record.Context != null && record.Context.Count > 0)
            {
                SetAlternateColor(color);
                for (int i = 0 ; i < record.Context.Count ; i++)
                {
                    Console.Write(" " + record.Context[i]);
                }
            }

            Console.WriteLine();
        }

        private void SetColor(ConsoleColor color)
        {
            if (_useColors)
            {
                Console.ForegroundColor = color;
            }
        }

        private void SetAlternateColor(ConsoleColor color)
        {
            if (_useColors)
            {
                Console.ForegroundColor = color switch 
                {
                    ConsoleColor.DarkGray => ConsoleColor.Gray,
                    ConsoleColor.Gray => ConsoleColor.White,
                    ConsoleColor.Cyan => ConsoleColor.Yellow,
                    ConsoleColor.Yellow => ConsoleColor.White,
                    ConsoleColor.Red => ConsoleColor.Yellow,
                    _ => ConsoleColor.DarkGray
                };
            }
        }

        private IDisposable? SaveColor()
        {
            return _useColors ? new RestoreColorOnExit() : null;
        }

        private class RestoreColorOnExit : IDisposable
        {
            private readonly ConsoleColor _savedColor;
            
            public RestoreColorOnExit()
            {
                _savedColor = Console.ForegroundColor;
            }

            public void Dispose()
            {
                Console.ForegroundColor = _savedColor;
            }
        }
    }
}