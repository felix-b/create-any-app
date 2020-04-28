using System;
using System.Collections.Generic;

namespace LLang.Abstractions.Languages
{
    public class LexicalDiagnostic : Diagnostic<char>
    {
        public LexicalDiagnostic(char input, Marker<char> marker, LexicalDiagnosticDescription description) 
            : base(input, marker, description)
        {
        }
    }

    public class LexicalDiagnosticDescription : DiagnosticDescription<char>
    {
        public LexicalDiagnosticDescription(string code, DiagnosticLevel level, Func<Diagnostic<char>, string> formatter)
            : base(code, level, formatter)
        {
        }
    }

    public class LexicalDiagnosticList : DiagnosticList<char>
    {
    }
}
