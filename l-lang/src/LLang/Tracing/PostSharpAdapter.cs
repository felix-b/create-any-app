#if ANALYSIS_TRACE 

using System;
using System.Diagnostics;
using System.Reflection;
using PostSharp.Aspects;

namespace LLang.Tracing
{
    [Serializable]
    [DebuggerNonUserCode]
    [DebuggerStepThrough]
    public class TracedAttribute : OnMethodBoundaryAspect
    {
        public override void OnEntry(MethodExecutionArgs args)
        {
            if (!RealTrace.Enabled)
            {
                return;
            }

            MethodBase method = args.Method;
            var typeName = method.DeclaringType?.Name ?? "unknown-type";
            var instanceName = args.Instance?.ToString() ?? typeName;
            
            var span = RealTrace.SingleInstance.Span(message: instanceName + "." + method.Name, WithArguments);
            args.MethodExecutionTag = span;

            ITraceContextBuilder WithArguments(ITraceContextBuilder context)
            {
                var arguments = args.Arguments;
                if (arguments != null)
                {
                    var formalParameters = method.GetParameters();
                    for (int i = 0 ; i < arguments.Count ; i++)
                    {
                        context.Add(formalParameters[i].Name, arguments[i]);
                    }
                }
                return context;
            }
        }

        public override void OnExit(MethodExecutionArgs args)
        {
            if (!RealTrace.Enabled)
            {
                return;
            }

            if (args.MethodExecutionTag is ITraceSpan span)
            {
                if (args.Exception != null)
                {
                    span.Failure(args.Exception);
                }
                else if (args.Method is MethodInfo methodInfo && methodInfo.ReturnType != typeof(void)) 
                {
                    span.ResultValue(args.ReturnValue);
                }
                span.Dispose();
            }
            else 
            {
                throw new Exception("RealTrace: expected ITraceSpan in MethodExecutionTag");
            }
        }
    }
}

#endif