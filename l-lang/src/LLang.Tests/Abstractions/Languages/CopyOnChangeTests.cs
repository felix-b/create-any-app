// using System;
// using LLang.Abstractions;
// using LLang.Abstractions.Languages;
// using NUnit.Framework;
// using FluentAssertions;
// using System.IO;
// using System.Linq;

// namespace LLang.Tests.Abstractions.Languages
// {
//     public class CopyOnChangeTests
//     {
//         [Test]
//         public void CreateEmptyGrammar()
//         {
//             var grammar0 = new Grammar<char, Token>();
//             grammar0.Rules.Should().BeEmpty();
//         }

//         [Test]
//         public void AddFirstRuleToGrammar()
//         {
//             var grammar0 = new Grammar<char, Token>();
//             var rule1 = new Rule<char, Token>("r1", (m, x) => new Token("t1", m, x));
            
//             var grammar1 = grammar0.WithRule(rule1, out var newRoot1);

//             CollectionAssert.AreEqual(new[] { rule1 }, grammar1.Rules);
//             rule1.ParentGrammar.Value.Should().BeSameAs(grammar1);
//             newRoot1.Should().BeSameAs(grammar1);
//         }

//         [Test]
//         public void AddSecondRuleToGrammar()
//         {
//             var grammar0 = new Grammar<char, Token>();
            
//             var grammar1 = grammar0.WithRule(new Rule<char, Token>("r1", (m, x) => new Token("t1", m, x)), out var newRoot1);
//             var grammar2 = grammar1.WithRule(new Rule<char, Token>("r2", (m, x) => new Token("t2", m, x)), out var newRoot2);

//             CollectionAssert.AreEqual(new[] { "r1" } , grammar1.Rules.Select(r => r.Id));
//             CollectionAssert.AreEqual(new[] { "r1", "r2" }, grammar2.Rules.Select(r => r.Id));
//             CollectionAssert.AreEqual(
//                 new[] { grammar2, grammar2 }, 
//                 grammar2.Rules.Select(r => r.ParentGrammar.Value)
//             );

//             newRoot1.Should().BeSameAs(grammar1);
//             newRoot2.Should().BeSameAs(grammar2);
//         }

//         [Test]
//         public void GetRuleById()
//         {
//             var grammar0 = new Grammar<char, Token>();
            
//             var grammar1 = grammar0.WithRule(new Rule<char, Token>("r1", (m, x) => new Token("t1", m, x)), out var newRoot1);
//             var grammar2 = grammar1.WithRule(new Rule<char, Token>("r2", (m, x) => new Token("t2", m, x)), out var newRoot2);

//             CollectionAssert.AreEqual(new[] { "r1" } , grammar1.Rules.Select(r => r.Id));
//             CollectionAssert.AreEqual(new[] { "r1", "r2" }, grammar2.Rules.Select(r => r.Id));
//             CollectionAssert.AreEqual(
//                 new[] { grammar2, grammar2 }, 
//                 grammar2.Rules.Select(r => r.ParentGrammar.Value)
//             );

//             newRoot1.Should().BeSameAs(grammar1);
//             newRoot2.Should().BeSameAs(grammar2);
//         }
//     }
// }