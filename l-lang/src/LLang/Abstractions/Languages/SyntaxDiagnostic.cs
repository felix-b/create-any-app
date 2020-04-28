using System;
using System.Collections.Generic;

namespace LLang.Abstractions.Languages
{
    public class SyntaxDiagnostic : Diagnostic<Token>
    {
        public SyntaxDiagnostic(Token input, Marker<Token> marker, SyntaxDiagnosticDescription description) 
            : base(input, marker, description)
        {
        }

        public SourceSpan Span => Input.Span;
    }

    public class SyntaxDiagnosticDescription : DiagnosticDescription<Token>
    {
        public SyntaxDiagnosticDescription(string code, DiagnosticLevel level, Func<Diagnostic<Token>, string> formatter)
            : base(code, level, formatter)
        {
        }
    }

    public class SyntaxDiagnosticList : DiagnosticList<Token>
    {
    }
}
