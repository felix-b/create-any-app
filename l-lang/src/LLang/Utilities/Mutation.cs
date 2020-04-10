// namespace LLang.Utilities
// {
//     public struct Mutation<T>
//     {
//         public Mutation(T newValue)
//         {
//             NewValue = newValue;
//         }

//         public T NewValue { get; }

//         public static implicit operator T(Mutation<T> mutation) => mutation.NewValue;

//         public static implicit operator Mutation<T> (T newValue) => new Mutation<T>(newValue);
//     }

//     public static class Mutation
//     {
//         public static Mutation<T> FromValue<T>(T newValue) => new Mutation<T>(newValue);
//         public static Mutation<T?> NullValue<T>() where T : struct => new Mutation<T?>(newValue: null);
//         public static Mutation<T?> NullObject<T>() where T : class => new Mutation<T?>(newValue: null);

//         public static T Or<T>(this Mutation<T>? mutation, T sourceValue)
//         {
//             return mutation.HasValue
//                 ? mutation.Value.NewValue
//                 : sourceValue;
//         }
//     }
// }
