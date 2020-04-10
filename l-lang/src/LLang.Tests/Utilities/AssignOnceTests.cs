// using System;
// using LLang.Utilities;
// using NUnit.Framework;
// using FluentAssertions;

// namespace LLang.Tests.Utilities
// {
//     [TestFixture]
//     public class AssisnOnceTests
//     {
//         [Test]
//         public void InitiallyNotAssigned()
//         {
//             var one = new One();
//             one.Two.HasValue.Should().BeFalse();
//         }

//         [Test]
//         public void ValueThrowsIfNotAssigned()
//         {
//             var one = new One();
//             Assert.Throws<InvalidOperationException>(() => {
//                 var value = one.Two.Value;
//             });
//         }

//         [Test]
//         public void CanAssignValue()
//         {
//             var one = new One();
//             var two = new Two();

//             one.Two.Assign(two);
            
//             one.Two.Value.Should().BeSameAs(two);
//         }

//         [Test]
//         public void CanConvertWithImplicitOperator()
//         {
//             var one = new One();
//             var two = new Two();

//             one.Two.Assign(two);
            
//             Two value = one.Two;
//             value.Should().BeSameAs(two);
//         }

//         [Test]
//         public void ImplicitOperatorThrowsIfValueNotAssigned()
//         {
//             var one = new One();
//             Assert.Throws<InvalidOperationException>(() => {
//                 Two value = one.Two;
//             });
//         }

//         public class One
//         {
//             public AssignOnce<Two> Two { get; } = new AssignOnce<Two>();
//         }

//         public class Two
//         {

//         }
//     }
// }
