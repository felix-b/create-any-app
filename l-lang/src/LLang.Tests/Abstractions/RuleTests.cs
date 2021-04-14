using System;
using LLang.Abstractions;
using LLang.Abstractions.Languages;
using NUnit.Framework;
using FluentAssertions;
using System.IO;
using LLang.Tracing;
using LLang.Utilities;
using static LLang.Tests.Abstractions.GrammarRepository;

namespace LLang.Tests.Abstractions
{
    [TestFixture]
    public class RuleTests
    {
        [Test]
        public void WithRecovery_SuccessfulMatch_ProduceToken()
        {
            var rule = RuleAa();
            var input = "Aa";
            var reader = new SourceFileReader(new NoopTrace(), "t.src", new StringReader(input));

            reader.ReadNextInput();
            var match = rule.TryMatchStart(reader) ?? throw new Exception("TryMatchStart failed");

            reader.ReadNextInput();
            match.Next(reader).Should().Be(true);

            reader.ReadNextInput();
            match.Next(reader).Should().Be(false);

            match.ValidateMatch(reader).Should().Be(true);
            match.Product.Value.Should().BeOfType<AToken>();
        }
    }
}