using PostSharp.Extensibility;
using PostSharp.Patterns.Diagnostics;
using PostSharp.Patterns.Diagnostics.Backends.Console;

[assembly: Log(
    AttributePriority = 1, 
    AttributeTargetTypes = "LLang.Abstractions.RuleMatch*",
    AttributeTargetElements = MulticastTargets.Method
)]
[assembly: Log(
    AttributePriority = 2, 
    AttributeTargetTypes = "LLang.Abstractions.ChoiceMatch*",
    AttributeTargetElements = MulticastTargets.Method
)]
[assembly: Log(
    AttributePriority = 10, 
    AttributeExclude = true, 
    AttributeTargetMembers = "get_*"
)]
[assembly: Log(
    AttributePriority = 11, 
    AttributeExclude = true, 
    AttributeTargetMembers = "set_*"
)]

namespace LLang.Tracing
{
    public static class AnalysisTrace
    {
        [Log(AttributeExclude = true)]
        public static void Initialize(bool useColors)
        {
            var backend = new ConsoleLoggingBackend();
            backend.Options.Theme = useColors ? ConsoleThemes.Dark : ConsoleThemes.None;
            LoggingServices.DefaultBackend = backend;
        }
    }
}
