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
    public class RecoveringMatchTests
    {
        [Test]
        public void SuccessfulMatch_Transparent()
        {
            var mainRule = RuleAa();
            var recoveryRule = AaBbRecoveryRule();
            
            var input = "Aa";
            var reader = new SourceFileReader(new NoopTrace(), "test.src", new StringReader(input));
            
            reader.ReadNextInput().Should().BeTrue();
            reader.Input.Should().Be('A');

            var mainMatch = mainRule.TryMatchStart(reader) ?? throw new Exception("mainRule.TryMatchStart failed!"); 
            var matchUnderTest = new RecoveringMatch<char, Token>(mainMatch, recoveryRule);

            reader.ReadNextInput().Should().Be(true);
            reader.Input.Should().Be('a');
            
            matchUnderTest.Next(reader).Should().BeTrue();
            reader.ReadNextInput().Should().Be(false);

            matchUnderTest.ValidateMatch(reader).Should().BeTrue();
            mainMatch.Product.Value.Should().BeOfType<AToken>();
            matchUnderTest.Product.Value.Should().BeSameAs(mainMatch.Product.Value);
        }
        
        [Test]
        public void FailedMatch_SuccessfulRecovery()
        {
            var mainRule = RuleAa();
            var recoveryRule = AaBbRecoveryRule();

            var input = "AXxBb";
            var reader = new SourceFileReader(new NoopTrace(), "test.src", new StringReader(input));
            reader.ReadNextInput().Should().BeTrue();
            reader.Input.Should().Be('A');

            var mainMatch = mainRule.TryMatchStart(reader) ?? throw new Exception("mainRule.TryMatchStart failed!"); 
            var matchUnderTest = new RecoveringMatch<char, Token>(mainMatch, recoveryRule);

            reader.ReadNextInput().Should().Be(true);
            reader.Input.Should().Be('X');
            matchUnderTest.Next(reader).Should().Be(true);

            reader.ReadNextInput().Should().Be(true);
            reader.Input.Should().Be('x');
            matchUnderTest.Next(reader).Should().Be(true);
            
            reader.ReadNextInput().Should().Be(true);
            reader.Input.Should().Be('B');
            matchUnderTest.Next(reader).Should().Be(false);

            matchUnderTest.ValidateMatch(reader).Should().Be(true);
            mainMatch.Product.HasValue.Should().Be(false);
            matchUnderTest.Product.Value.Should().BeOfType<ErrToken>();
        }

        [Test]
        public void FailedMatch_FailedRecovery()
        {
            var mainRule = RuleAa();
            var recoveryRule = new Rule<char, Token>("E", new IState<char>[] {
                CharRangeState.Create("r1", negating: true, Quantifier.Exactly(2), LexerUtility.CharRangesFromString("AB"))
            }, m => new ErrToken(m));

            var input = "AXB";
            var reader = new SourceFileReader(new NoopTrace(), "test.src", new StringReader(input));
            reader.ReadNextInput().Should().BeTrue();
            reader.Input.Should().Be('A');

            var mainMatch = mainRule.TryMatchStart(reader) ?? throw new Exception("mainRule.TryMatchStart failed!"); 
            var matchUnderTest = new RecoveringMatch<char, Token>(mainMatch, recoveryRule);

            reader.ReadNextInput().Should().Be(true);
            reader.Input.Should().Be('X');
            matchUnderTest.Next(reader).Should().Be(true);

            reader.ReadNextInput().Should().Be(true);
            reader.Input.Should().Be('B');
            matchUnderTest.Next(reader).Should().Be(false);

            matchUnderTest.ValidateMatch(reader).Should().Be(false);
            mainMatch.Product.HasValue.Should().Be(false);
            matchUnderTest.Product.HasValue.Should().Be(false);
        }

    }
}
