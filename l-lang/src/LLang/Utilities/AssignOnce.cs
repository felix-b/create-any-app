// using System;

// namespace LLang.Utilities
// {
//     public class AssignOnce<T>
//         where T : class
//     {
//         private T? _value = null;
//         public T Assign(T value)
//         {
//             if (_value != null)
//             {
//                 throw new InvalidOperationException("Value already assigned");
//             }
//             _value = value;
//             return value;
//         }

//         public T Value => _value ?? throw new InvalidOperationException("Value was not assigned");
//         public bool HasValue => _value != null;

//         public static implicit operator T(AssignOnce<T> instance) => instance.Value;
//     }
// }
