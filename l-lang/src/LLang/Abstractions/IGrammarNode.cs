// using System.Collections.Generic;

// namespace LLang.Abstractions
// {
//     public interface IGrammarNode<TIn, TOut>
//     {
//     }

//     public interface IGrammarNode<TIn, TOut, TThis, TParent> : IGrammarNode<TIn, TOut>
//         where TThis : class, IGrammarNode<TIn, TOut>
//         where TParent : class, IGrammarNode<TIn, TOut>
//     {
//         void AttachParent(TParent parent);
//         Choice<TIn, TOut> ReplaceWith(TThis newVersion);
//         TParent? Parent { get; }
//     }
// }
