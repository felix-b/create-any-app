using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using LLang.Abstractions;
using LLang.Abstractions.Languages;

namespace LLang.Demos.Json
{
    public static class JsonGrammar
    {
        public static Grammar<char, Token> CreateLexicon()
        {
            var grammar = new Grammar<char, Token>("json-lex");
            grammar.Build()
                .Rule(WhitespaceToken.Create, r => r.CharRange(" \r\n\t", Quantifier.Any))
                .Rule(OpenObjectToken.Create, r => r.Char('{'))
                .Rule(CloseObjectToken.Create, r => r.Char('}'))
                .Rule(OpenArrayToken.Create, r => r.Char('['))
                .Rule(CloseArrayToken.Create, r => r.Char(']'))
                .Rule(ColonToken.Create, r => r.Char(':'))
                .Rule(CommaToken.Create, r => r.Char(','))
                .Rule(NumberToken.Create, r => r
                    .CharRange("+-", Quantifier.AtMostOnce)
                    .CharRange(('0','9'), Quantifier.AtLeastOnce)
                    .Group(
                        "fraction",
                        Token.CreateToken,
                        r => r.Char('.').CharRange(('0','9'), Quantifier.AtLeastOnce), 
                        Quantifier.AtMostOnce
                    )
                    .Group(
                        "exponent",
                        Token.CreateToken,
                        r => r
                            .CharRange("eE", Quantifier.Once)
                            .CharRange("+-", Quantifier.AtMostOnce)
                            .CharRange(('0','9'), Quantifier.AtLeastOnce),
                        Quantifier.AtMostOnce
                    )
                )
                .Rule(StringToken.Create, r => r
                    .Char('"')
                    .Choice("value", g => g
                        .Rule("non-esc-text", NonEscapedTextToken.Create, r => r
                            .NotCharRange("\"\\", Quantifier.Any)
                        )
                        .Rule("esc-char", EscapeSequenceToken.CreateFromChar, r => r
                            .Char('\\')
                            .AnyChar()
                        )
                        .Rule("esc-utf16", EscapeSequenceToken.CreateFromHex, r => r
                            .String("\\u")
                            .CharRange(new[] { ('0','9'),('a','f'),('A','F') }, Quantifier.Exactly(4))
                        ),
                        Quantifier.Any
                    )
                    .Char('"')
                )
                .Rule(TrueToken.Create, r => r.String("true"))
                .Rule(FalseToken.Create, r => r.String("false"))
                .Rule(NullToken.Create, r => r.String("null"));

            return grammar;
        }

        public static Grammar<Token, SyntaxNode> CreateSyntax()
        {
            var valueRule = new Rule<Token, SyntaxNode>("value", ValueSyntax.ConstructAnyValue);
            var arrayRule = new Rule<Token, SyntaxNode>("array", ArraySyntax.FindInSubRules);
            var objectRule = new Rule<Token, SyntaxNode>("object", ObjectSyntax.FindInSubRules);
            var propertyRule = new Rule<Token, SyntaxNode>("property", PropertySyntax.Construct);
            var propertyListRule = new Rule<Token, SyntaxNode>("property-list", SyntaxList.ConstructOfType<PropertySyntax>);
            var itemListRule = new Rule<Token, SyntaxNode>("item-list", SyntaxList.ConstructOfType<ValueSyntax>);

            propertyListRule.Build().Choice(g => g
                .Rule(propertyRule)
                .Rule("property-list-prepend", SyntaxList.Construct, r => r
                    .Group("property-list-prepend",
                        SyntaxList.Construct, 
                        r => r.Rule(propertyRule).Token<CommaToken>().Rule(propertyListRule),
                        Quantifier.Once)
                )
            );

            itemListRule.Build().Choice(g => g
                .Rule(valueRule)
                .Rule("item-list-prepend", SyntaxList.Construct, r => r
                    .Group("item-list-prepend",
                        SyntaxList.Construct, 
                        r => r.Rule(valueRule).Token<CommaToken>().Rule(itemListRule),
                        Quantifier.Once)
                )
            );

            valueRule.Build().Choice(g => g
                .Rule("number-value", ScalarValueSyntax.Construct, r => r.Token<NumberToken>())
                .Rule("string-value", ScalarValueSyntax.Construct, r => r.Token<StringToken>())
                .Rule("true-value", ScalarValueSyntax.Construct, r => r.Token<TrueToken>())
                .Rule("false-value", ScalarValueSyntax.Construct, r => r.Token<FalseToken>())
                .Rule(arrayRule)
                .Rule(objectRule)
            );

            arrayRule.Build().Choice(g => g
                .Rule("empty", ArraySyntax.ConstructEmpty, r => r
                    .Token<OpenArrayToken>()
                    .Token<CloseArrayToken>())
                .Rule("non-empty", ArraySyntax.ConstructFromItemList, r => r
                    .Token<OpenArrayToken>()
                    .Rule(itemListRule)
                    .Token<CloseArrayToken>()
                    // .Token<OpenArrayToken>()
                    // .Rule("first-item", valueRule)
                    // .Group("more-items", 
                    //     ValueSyntax.RetrieveFromList, 
                    //     r => r.Token<CommaToken>().Rule(valueRule),
                    //     Quantifier.Any)
                    // .Token<CloseArrayToken>()
                )
            );

            objectRule.Build().Choice(g => g
                .Rule("empty", ObjectSyntax.ConstructEmpty, r => r
                    .Token<OpenObjectToken>()
                    .Token<CloseObjectToken>())
                .Rule("non-empty", ObjectSyntax.ConstructFromPropertyList, r => r
                    .Token<OpenObjectToken>()
                    .Rule(propertyListRule)
                    .Token<CloseObjectToken>()
                    // .Token<OpenObjectToken>()
                    // .Rule("first-prop", propertyRule)
                    // .Group("more-props", 
                    //     PropertySyntax.RetrieveFromList, 
                    //     r => r.Token<CommaToken>().Rule(propertyRule),
                    //     Quantifier.Any)
                    // .Token<CloseObjectToken>()
                )
            );

            propertyRule.Build()
                .Token<StringToken>("name")
                .Token<ColonToken>()
                .Rule(valueRule);

            var grammar = new Grammar<Token, SyntaxNode>("json-syn", valueRule);
            return grammar;
        }

        public static Preprocessor CreatePreprocessor() =>
            source => 
                source.Where(t => !(t is WhitespaceToken));

        public interface IScalarToken
        {
            object? ClrValue { get; }
        }

        public class WhitespaceToken : Token, IProductOfFactory<WhitespaceToken.Factory>
        {
            public WhitespaceToken(IMatch<char> match, IInputContext<char> context)
                : base("WS", match, context)
            {
            }

            public class Factory : IProductFactory<char, Token>
            {
                public Token Create(RuleMatch<char, Token> match, IInputContext<char> context)
                {
                    return new WhitespaceToken(match, context);
                }
            }

            public static WhitespaceToken Create(IMatch<char> match, IInputContext<char> context)
            {
                return new WhitespaceToken(match, context);
            }
        }

        public class NullToken : Token, IScalarToken
        {
            public NullToken(IMatch<char> match, IInputContext<char> context)
                : base("NULL", match, context)
            {
            }

            public object? ClrValue => null;

            public static NullToken Create(IMatch<char> match, IInputContext<char> context)
            {
                return new NullToken(match, context);
            }
        }

        public class NumberToken : Token, IScalarToken, IProductOfFactory<NumberToken.Factory>
        {
            public NumberToken(IMatch<char> match, IInputContext<char> context)
                : base("NUM", match, context)
            {
                Value = decimal.Parse(Span.GetText());
            }

            public decimal Value { get; }

            public object ClrValue => Value;

            public class Factory : IProductFactory<char, Token>
            {
                public Token Create(RuleMatch<char, Token> match, IInputContext<char> context)
                {
                    return new NumberToken(match, context);
                }
            }

            public static NumberToken Create(IMatch<char> match, IInputContext<char> context)
            {
                return new NumberToken(match, context);
            }
        }

        public class TrueToken : Token, IScalarToken, IProductOfFactory<TrueToken.Factory>
        {
            public TrueToken(IMatch<char> match, IInputContext<char> context)
                : base("TRUE", match, context)
            {
            }

            public object ClrValue => true;

            public class Factory : IProductFactory<char, Token>
            {
                public Token Create(RuleMatch<char, Token> match, IInputContext<char> context)
                {
                    return new TrueToken(match, context);
                }
            }

            public static TrueToken Create(IMatch<char> match, IInputContext<char> context)
            {
                return new TrueToken(match, context);
            }
        }

        public class FalseToken : Token, IScalarToken, IProductOfFactory<FalseToken.Factory>
        {
            public FalseToken(IMatch<char> match, IInputContext<char> context)
                : base("FALSE", match, context)
            {
            }

            public object ClrValue => false;

            public class Factory : IProductFactory<char, Token>
            {
                public Token Create(RuleMatch<char, Token> match, IInputContext<char> context)
                {
                    return new FalseToken(match, context);
                }
            }

            public static FalseToken Create(IMatch<char> match, IInputContext<char> context)
            {
                return new FalseToken(match, context);
            }
        }

        public class StringToken : Token, IScalarToken, IProductOfFactory<StringToken.Factory>
        {
            public StringToken(IEnumerable<IStringContentToken> contentTokens, RuleMatch<char, Token> match, IInputContext<char> context)
                : base("STR", match, context)
            {
                ContentTokens = contentTokens.ToArray();
                Value = string.Join(string.Empty, ContentTokens.Select(t => t.UnescapedText));
            }

            public IReadOnlyList<IStringContentToken> ContentTokens { get; }
            public string Value { get; }
            public object ClrValue => Value;


            public class Factory : IProductFactory<char, Token>
            {
                public Token Create(RuleMatch<char, Token> match, IInputContext<char> context)
                {
                    return StringToken.Create(match, context);
                }
            }

            public static StringToken Create(RuleMatch<char, Token> match, IInputContext<char> context)
            {
                var choice = match.FindChoiceByStateId("value") 
                    ?? throw new Exception("StringToken: cannot construct (1)");
                
                var tokens = choice!                    
                    .GrammarMatches
                    .Select(m => m.MatchedRule?.Product.Value)
                    .OfType<IStringContentToken>()
                    ?? throw new Exception("StringToken: cannot construct (2)");

                return new StringToken(tokens, match, context);
            }
        }

        public interface IStringContentToken
        {
            string UnescapedText { get; }
            Token AsToken { get; }
        }

        public class NonEscapedTextToken : Token, IStringContentToken
        {
            public NonEscapedTextToken(IMatch<char> match, IInputContext<char> context) 
                : base(name: "non-esc-text", match, context)
            {
            }

            public string UnescapedText => Span.GetText();
            public Token AsToken => this;

            public static NonEscapedTextToken Create(RuleMatch<char, Token> match, IInputContext<char> context)
            {
                return new NonEscapedTextToken(match, context);
            }
        }

        public class EscapeSequenceToken : Token, IStringContentToken
        {
            private static readonly string _escapableCharIndex = "bfnrt";
            private static readonly string _escapedCharIndex = "\b\f\n\r\t";

            public EscapeSequenceToken(char escapedChar, IMatch<char> match, IInputContext<char> context)
                : base(name: "escape", match, context)
            {
                EscapedChar = escapedChar;
            }

            public char EscapedChar { get; }
            public string UnescapedText => new string(EscapedChar, 1);
            public Token AsToken => this;

            public static EscapeSequenceToken CreateFromChar(RuleMatch<char, Token> match, IInputContext<char> context)
            {
                var text = context.GetSlice(match.StartMarker, match.EndMarker).ToString();
                if (text.Length == 2 && text[0] == '\\')
                {
                    var index = _escapableCharIndex.IndexOf(text[1]);
                    var escapedChar = index >= 0 ? _escapedCharIndex[index] : text[1];
                    return new EscapeSequenceToken(escapedChar, match, context);
                }
    
                throw new Exception("EscapedCharToken: cannot construct (1)");
            }

            public static EscapeSequenceToken CreateFromHex(RuleMatch<char, Token> match, IInputContext<char> context)
            {
                var text = context.GetSlice(match.StartMarker, match.EndMarker).ToString();
                if (text.Length == 6 && int.TryParse(text[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var charCode))
                {
                    return new EscapeSequenceToken(escapedChar: (char)charCode, match, context);
                }
                    
                throw new Exception("EscapedCharToken: cannot construct (1)");
            }
        }

        public class OpenObjectToken : Token, IProductOfFactory<OpenObjectToken.Factory>
        {
            public OpenObjectToken(IMatch<char> match, IInputContext<char> context)
                : base("OPENOBJ", match, context)
            {
            }

            public class Factory : IProductFactory<char, Token>
            {
                public Token Create(RuleMatch<char, Token> match, IInputContext<char> context)
                {
                    return new OpenObjectToken(match, context);
                }
            }

            public static OpenObjectToken Create(IMatch<char> match, IInputContext<char> context)
            {
                return new OpenObjectToken(match, context);
            }
        }

        public class CloseObjectToken : Token, IProductOfFactory<CloseObjectToken.Factory>
        {
            public CloseObjectToken(IMatch<char> match, IInputContext<char> context)
                : base("CLOSEOBJ", match, context)
            {
            }

            public class Factory : IProductFactory<char, Token>
            {
                public Token Create(RuleMatch<char, Token> match, IInputContext<char> context)
                {
                    return new CloseObjectToken(match, context);
                }
            }

            public static CloseObjectToken Create(IMatch<char> match, IInputContext<char> context)
            {
                return new CloseObjectToken(match, context);
            }
        }

        public class OpenArrayToken : Token, IProductOfFactory<OpenArrayToken.Factory>
        {
            public OpenArrayToken(IMatch<char> match, IInputContext<char> context)
                : base("OPENARR", match, context)
            {
            }

            public class Factory : IProductFactory<char, Token>
            {
                public Token Create(RuleMatch<char, Token> match, IInputContext<char> context)
                {
                    return new OpenArrayToken(match, context);
                }
            }

            public static OpenArrayToken Create(IMatch<char> match, IInputContext<char> context)
            {
                return new OpenArrayToken(match, context);
            }
        }

        public class CloseArrayToken : Token, IProductOfFactory<CloseArrayToken.Factory>
        {
            public CloseArrayToken(IMatch<char> match, IInputContext<char> context)
                : base("CLOSEARR", match, context)
            {
            }

            public class Factory : IProductFactory<char, Token>
            {
                public Token Create(RuleMatch<char, Token> match, IInputContext<char> context)
                {
                    return new CloseArrayToken(match, context);
                }
            }

            public static CloseArrayToken Create(IMatch<char> match, IInputContext<char> context)
            {
                return new CloseArrayToken(match, context);
            }
        }

        public class ColonToken : Token, IProductOfFactory<ColonToken.Factory>
        {
            public ColonToken(IMatch<char> match, IInputContext<char> context)
                : base("COLON", match, context)
            {
            }

            public class Factory : IProductFactory<char, Token>
            {
                public Token Create(RuleMatch<char, Token> match, IInputContext<char> context)
                {
                    return new ColonToken(match, context);
                }
            }

            public static ColonToken Create(IMatch<char> match, IInputContext<char> context)
            {
                return new ColonToken(match, context);
            }
        }

        public class CommaToken : Token, IProductOfFactory<CommaToken.Factory>
        {
            public CommaToken(IMatch<char> match, IInputContext<char> context)
                : base("COMMA", match, context)
            {
            }

            public class Factory : IProductFactory<char, Token>
            {
                public Token Create(RuleMatch<char, Token> match, IInputContext<char> context)
                {
                    return new CommaToken(match, context);
                }
            }

            public static CommaToken Create(IMatch<char> match, IInputContext<char> context)
            {
                return new CommaToken(match, context);
            }
        }

        public class ObjectSyntax : SyntaxNode
        {
            public ObjectSyntax(SourceSpan span, IEnumerable<PropertySyntax> properties) 
                : base(span, properties)
            {
                Properties = properties.ToArray();
            }

            public IReadOnlyList<PropertySyntax> Properties { get; }

            public static ObjectSyntax FindInSubRules(RuleMatch<Token, SyntaxNode> match, IInputContext<Token> context)
            {
                var choiceMatch = match.MatchedStates.SingleOrDefault() as IChoiceRefStateMatch<Token, SyntaxNode>
                    ?? throw new Exception("ObjectSyntax: cannot find in sub-rules (1)");

                return choiceMatch.FindSingleRuleProductOrThrow<ObjectSyntax>();
            }

            public static ObjectSyntax ConstructEmpty(RuleMatch<Token, SyntaxNode> match, IInputContext<Token> context)
            {
                return new ObjectSyntax(
                    SourceSpan.FromTokens(match, context), 
                    Array.Empty<PropertySyntax>());
            }

            public static ObjectSyntax ConstructNonEmpty(RuleMatch<Token, SyntaxNode> match, IInputContext<Token> context)
            {
                var firstProp = match.FindRuleByStateId("first-prop")?.FindSingleRuleProductOrThrow<PropertySyntax>()
                    ?? throw new Exception("ObjectSyntax: cannot construct non-empty (1)");
                
                var moreProps = match.FindRuleByStateId("more-props")?.RuleMatches
                    .Where(r => r.Product.HasValue)
                    .Select(r => r.Product.Value)
                    .OfType<PropertySyntax>()
                    .ToArray()
                    ?? throw new Exception("ObjectSyntax: cannot construct non-empty (2)");

                return new ObjectSyntax(
                    SourceSpan.FromTokens(match, context), 
                    moreProps.Prepend(firstProp));
            }

            public static ObjectSyntax ConstructFromPropertyList(RuleMatch<Token, SyntaxNode> match, IInputContext<Token> context)
            {
                var properties = match.FindRuleByStateId("property-list")?.FindSingleRuleProductOrThrow<SyntaxList>()
                    ?? throw new Exception("ObjectSyntax: cannot construct from prop list (1)");

                return new ObjectSyntax(
                    SourceSpan.FromTokens(match, context), 
                    properties.Cast<PropertySyntax>()
                );
            }
        }

        public class ArraySyntax : SyntaxNode
        {
            public ArraySyntax(SourceSpan span, IEnumerable<ValueSyntax> items) 
                : base(span, items)
            {
                Items = items.ToArray();
            }

            public IReadOnlyList<ValueSyntax> Items { get; }

            public static ArraySyntax FindInSubRules(RuleMatch<Token, SyntaxNode> match, IInputContext<Token> context)
            {
                var choiceMatch = match.MatchedStates.SingleOrDefault() as IChoiceRefStateMatch<Token, SyntaxNode>
                    ?? throw new Exception("ArraySyntax: cannot find in sub-rules (1)");

                return choiceMatch.FindSingleRuleProductOrThrow<ArraySyntax>();
            }

            public static ArraySyntax ConstructEmpty(RuleMatch<Token, SyntaxNode> match, IInputContext<Token> context)
            {
                return new ArraySyntax(
                    SourceSpan.FromTokens(match, context), 
                    Array.Empty<ValueSyntax>());
            }

            public static ArraySyntax ConstructNonEmpty(RuleMatch<Token, SyntaxNode> match, IInputContext<Token> context)
            {
                var firstItem = match.FindRuleByStateId("first-item")?.FindSingleRuleProductOrThrow<ValueSyntax>()
                    ?? throw new Exception("ArraySyntax: cannot construct non-empty (1)");
                
                var moreItems = match.FindRuleByStateId("more-items")?.RuleMatches
                    .Where(r => r.Product.HasValue)
                    .Select(r => r.Product.Value)
                    .OfType<ValueSyntax>()
                    .ToArray()
                    ?? throw new Exception("ArraySyntax: cannot construct non-empty (2)");

                return new ArraySyntax(
                    SourceSpan.FromTokens(match, context), 
                    moreItems.Prepend(firstItem));
            }

            public static ArraySyntax ConstructFromItemList(RuleMatch<Token, SyntaxNode> match, IInputContext<Token> context)
            {
                var items = match.FindRuleByStateId("item-list")?.FindSingleRuleProductOrThrow<SyntaxList>()
                    ?? throw new Exception("ArraySyntax: cannot construct from item list (1)");

                return new ArraySyntax(
                    SourceSpan.FromTokens(match, context), 
                    items.Cast<ValueSyntax>()
                );
            }
        }

        public class PropertySyntax : SyntaxNode
        {
            public PropertySyntax(SourceSpan span, StringToken nameToken, ValueSyntax valueSyntax) 
                : base(span, new SyntaxNode[] { valueSyntax })
            {
                Name = nameToken.Value;
                NameToken = nameToken;
                ValueSyntax = valueSyntax;
            }

            public string Name { get; }
            public StringToken NameToken { get; }
            public ValueSyntax ValueSyntax { get; }

            public static PropertySyntax Construct(RuleMatch<Token, SyntaxNode> match, IInputContext<Token> context)
            {
                var nameToken = match.FindStateByIdOrThrow<TokenState>("name").Input as StringToken
                    ?? throw new Exception("PropertySyntax: cannot construct (1)");

                var valueSyntax = match.FindRuleById("value")?.FindSingleRuleProductOrThrow<ValueSyntax>()
                    ?? throw new Exception("PropertySyntax: cannot construct (2)");

                return new PropertySyntax(
                    SourceSpan.FromTokens(match, context), 
                    nameToken,
                    valueSyntax);
            }

            public static PropertySyntax RetrieveFromList(RuleMatch<Token, SyntaxNode> match, IInputContext<Token> context)
            {
                var syntax = match.FindRuleById("property")?.FindSingleRuleProductOrThrow<PropertySyntax>()
                    ?? throw new Exception("PropertySyntax: cannot retrieve from list (1)");

                return syntax;
            }
        }

        public abstract class ValueSyntax : SyntaxNode
        {
            protected ValueSyntax(SourceSpan span) 
                : base(span, children: Array.Empty<SyntaxNode>())
            {
            }

            public static ValueSyntax ConstructAnyValue(RuleMatch<Token, SyntaxNode> match, IInputContext<Token> context)
            {
                var span = SourceSpan.FromTokens(match, context);
                var choiceMatch = match.MatchedStates.SingleOrDefault() as IChoiceRefStateMatch<Token, SyntaxNode>
                    ?? throw new Exception("ValueSyntax: cannot construct (1)");

                var literalSyntax = choiceMatch.FindSingleRuleProductOrThrow<SyntaxNode>();
                
                ValueSyntax concreteSyntax = literalSyntax switch
                {
                    ScalarValueSyntax scalar => scalar,
                    ObjectSyntax obj => new ObjectValueSyntax(span, obj),
                    ArraySyntax arr => new ArrayValueSyntax(span, arr),
                    _ => throw new Exception("ValueSyntax: cannot construct (2)")
                };

                return concreteSyntax;
            }

            public static ValueSyntax RetrieveFromList(RuleMatch<Token, SyntaxNode> match, IInputContext<Token> context)
            {
                var syntax = match.FindRuleById("value")?.FindSingleRuleProductOrThrow<ValueSyntax>()
                    ?? throw new Exception("PropertySyntax: cannot retrieve from list (1)");

                return syntax;
            }
        }

        public class ScalarValueSyntax : ValueSyntax
        {
            public ScalarValueSyntax(SourceSpan span, IScalarToken scalarToken) 
                : base(span)
            {
                ScalarToken = scalarToken;
            }

            public IScalarToken ScalarToken { get; }

            public static ScalarValueSyntax Construct(RuleMatch<Token, SyntaxNode> match, IInputContext<Token> context)
            {
                var scalarToken = match.MatchedStates.SingleOrDefault()?.Input as IScalarToken
                    ?? throw new Exception("ScalarValueSyntax: cannot construct (1)");

                return new ScalarValueSyntax(
                    SourceSpan.FromTokens(match, context), 
                    scalarToken);
            }
        }

        public class ObjectValueSyntax : ValueSyntax
        {
            public ObjectValueSyntax(SourceSpan span, ObjectSyntax objectSyntax) 
                : base(span)
            {
                ObjectSyntax = objectSyntax;
            }

            public ObjectSyntax ObjectSyntax { get; }
        }

        public class ArrayValueSyntax : ValueSyntax
        {
            public ArrayValueSyntax(SourceSpan span, ArraySyntax arraySyntax) 
                : base(span)
            {
                ArraySyntax = arraySyntax;
            }

            public ArraySyntax ArraySyntax { get; }
        }
    }
}
 