using System;
using LLang.Abstractions;
using LLang.Abstractions.Languages;
using NUnit.Framework;
using FluentAssertions;
using System.IO;
using System.Linq;
using LLang.Tracing;

namespace LLang.Tests.Abstractions
{
    [TestFixture]
    public class DiagnosticListTests
    {
        [Test]
        public void InitiallyEmpty()
        {
            var list = new DiagnosticList<char>();
            
            list.Count.Should().Be(0);
            list.ToArray().Should().BeEmpty();
            list.BacktrackLabels.Count.Should().Be(0);
            list.FurthestBacktrackLabel.Should().BeNull();
        }

        [Test]
        public void CanAddDiagnostics()
        {
            DiagnosticList<char> list = new DiagnosticList<char>();
            var diagnostic1 = new Diagnostic<char>('X', new Marker<char>(111), TestError);
            var diagnostic2 = new Diagnostic<char>('Y', new Marker<char>(222), TestError);

            list.AddDiagnostic(diagnostic1);
            list.AddDiagnostic(diagnostic2);

            list.Count.Should().Be(2);
            CollectionAssert.AreEqual(new[] { diagnostic1, diagnostic2 }, list);
            list.BacktrackLabels.Count.Should().Be(0);
        }

        [Test]
        public void CanAddBacktrackLabels()
        {
            DiagnosticList<char> list = new DiagnosticList<char>();
            var label1 = new BacktrackLabel<char>(new Marker<char>(111), new BacktrackLabelDescription<char>(TestError));
            var label2 = new BacktrackLabel<char>(new Marker<char>(222), new BacktrackLabelDescription<char>(TestError));
            list.AddBacktrackLabel(label1);
            list.AddBacktrackLabel(label2);

            list.BacktrackLabels.Count.Should().Be(2);
            CollectionAssert.AreEqual(new[] { label1, label2 }, list.BacktrackLabels);
            list.FurthestBacktrackLabel.Should().BeSameAs(label2);
        }

        [Test]
        public void CanDetermineFurthestBacktrackLabel()
        {
            DiagnosticList<char> list = new DiagnosticList<char>();
            var label1 = new BacktrackLabel<char>(new Marker<char>(111), new BacktrackLabelDescription<char>(TestError));
            var label2 = new BacktrackLabel<char>(new Marker<char>(999), new BacktrackLabelDescription<char>(TestError));
            var label3 = new BacktrackLabel<char>(new Marker<char>(333), new BacktrackLabelDescription<char>(TestError));

            list.AddBacktrackLabel(label1);
            list.AddBacktrackLabel(label2);
            list.AddBacktrackLabel(label3);

            list.FurthestBacktrackLabel.Should().BeSameAs(label2);
        }

        [Test]
        public void CanClearBacktrackLabels()
        {
            DiagnosticList<char> list = new DiagnosticList<char>();
            var label1 = new BacktrackLabel<char>(new Marker<char>(199), new BacktrackLabelDescription<char>(TestError));
            var label2 = new BacktrackLabel<char>(new Marker<char>(200), new BacktrackLabelDescription<char>(TestError));
            var label3 = new BacktrackLabel<char>(new Marker<char>(999), new BacktrackLabelDescription<char>(TestError));
            var label4 = new BacktrackLabel<char>(new Marker<char>(150), new BacktrackLabelDescription<char>(TestError));
            list.AddBacktrackLabel(label1);
            list.AddBacktrackLabel(label2);
            list.AddBacktrackLabel(label3);
            list.AddBacktrackLabel(label4);

            list.ClearBacktrackLabels(untilMarker: new Marker<char>(200));

            CollectionAssert.AreEqual(new[] { null, label2, label3, null }, list.BacktrackLabels);
            list.FurthestBacktrackLabel.Should().BeSameAs(label3);
        }

        [Test]
        public void CanClearAllBacktrackLabels()
        {
            DiagnosticList<char> list = new DiagnosticList<char>();
            var label1 = new BacktrackLabel<char>(new Marker<char>(199), new BacktrackLabelDescription<char>(TestError));
            var label2 = new BacktrackLabel<char>(new Marker<char>(200), new BacktrackLabelDescription<char>(TestError));
            list.AddBacktrackLabel(label1);
            list.AddBacktrackLabel(label2);

            list.ClearBacktrackLabels(untilMarker: new Marker<char>(300));

            CollectionAssert.AreEqual(new BacktrackLabel<char>?[] { null, null }, list.BacktrackLabels);
            list.FurthestBacktrackLabel.Should().BeNull();
        }

        private static readonly TestErrorDescription TestError = new TestErrorDescription();
        private static readonly TestWarningDescription TestWarning = new TestWarningDescription();
        private static readonly TestHintDescription TestHint = new TestHintDescription();

        private class TestErrorDescription : DiagnosticDescription<char>
        {
            public TestErrorDescription() : base("E1", DiagnosticLevel.Error, (diag) => "TEST_ERROR")
            {
            }
        }

        private class TestWarningDescription : DiagnosticDescription<char>
        {  
            public TestWarningDescription() : base("W1", DiagnosticLevel.Warning, (diag) => "TEST_WARNING")
            {
            }
        }

        private class TestHintDescription : DiagnosticDescription<char>
        {
            public TestHintDescription() : base("H1", DiagnosticLevel.Hint, (diag) => "TEST_HINT")
            {
            }
        }
    }
}
