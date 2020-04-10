// using System;
// using LLang.Utilities;
// using NUnit.Framework;
// using FluentAssertions;

// namespace LLang.Tests.Utilities
// {
//     [TestFixture]
//     public class MutationTests
//     {
//         [Test]
//         public void CanCopyWithSingleMutation()
//         {
//             IObj obj1 = new Obj();
//             IObj obj2 = new Obj();
//             var original = new One(
//                 str: "abc", 
//                 num: 123, 
//                 obj: obj1, 
//                 nullableStr: null, 
//                 nullableNum: null
//             );
            
//             var copy1 = new One(original, str: "def");
//             var copy2 = new One(original, num: 456);
//             var copy3 = new One(original, obj: Mutation.FromValue(obj2));

//             copy1.Str.Should().Be("def");
//             copy1.Num.Should().Be(123);
//             copy1.Obj.Should().BeSameAs(obj1);

//             copy2.Str.Should().Be("abc");
//             copy2.Num.Should().Be(456);
//             copy2.Obj.Should().BeSameAs(obj1);

//             copy3.Str.Should().Be("abc");
//             copy3.Num.Should().Be(123);
//             copy3.Obj.Should().BeSameAs(obj2);
//         }

//         [Test]
//         public void CanResetNullableToNull()
//         {
//             IObj obj1 = new Obj();
//             var original = new One(
//                 str: "abc", 
//                 num: 123, 
//                 obj: obj1,
//                 nullableStr: "not-null",
//                 nullableNum: 12345);
            
//             var copy1 = new One(original, nullableStr: Mutation.NullObject<string>());
//             var copy2 = new One(original, nullableNum: Mutation.NullValue<int>());

//             copy1.NullableStr.Should().BeNull();
//             copy1.NullableNum.GetValueOrDefault(-1).Should().Be(12345);

//             copy2.NullableStr.Should().Be("not-null");
//             copy2.NullableNum.GetValueOrDefault(-1).Should().Be(-1);
//         }

//         [Test]
//         public void AsssignableOnceIsResetOnCopy()
//         {
//             IObj obj1 = new Obj();
//             Two two1 = new Two();
//             var original = new One(
//                 str: "abc", 
//                 num: 123, 
//                 obj: obj1, 
//                 nullableStr: null, 
//                 nullableNum: null
//             );
//             original.Two.Assign(two1);
            
//             var copy = new One(original, str: "def");

//             original.Two.HasValue.Should().BeTrue();
//             copy.Two.HasValue.Should().BeFalse();
//         }

//         public class One
//         {
//             public One(string str, int num, IObj obj, string? nullableStr, int? nullableNum)
//             {
//                 Str = str;
//                 Num = num;
//                 Obj = obj;
//                 NullableStr = nullableStr;
//                 NullableNum = nullableNum;
//             }
//             public One(
//                 One source, 
//                 Mutation<string>? str = null, 
//                 Mutation<int>? num = null,
//                 Mutation<IObj>? obj = null,
//                 Mutation<string?>? nullableStr = null, 
//                 Mutation<int?>? nullableNum = null)
//             {
//                 Str = str ?? source.Str;
//                 Num = num ?? source.Num;
//                 Obj = obj.Or(source.Obj);
//                 NullableStr = nullableStr ?? source.NullableStr;
//                 NullableNum = nullableNum ?? source.NullableNum;
//             }
//             public string Str { get; }
//             public int Num { get; }
//             public IObj Obj { get; }
//             public string? NullableStr { get; }
//             public int? NullableNum { get; }
//             public AssignOnce<Two> Two { get; } = new AssignOnce<Two>();
//         }

//         public class Two
//         {
//         }

//         public interface IObj
//         {
//         }

//         public class Obj : IObj
//         {
//         }
//     }
// }