using System;
using System.Collections.Generic;
using System.Linq;
using LLang.Abstractions.Languages;
using static LLang.Tests.Demos.Json.JsonGrammar;

namespace LLang.Tests.Demos.Json
{
    public class JsonSemanticModel
    {
        private static readonly Dictionary<Type, Func<SyntaxNode, ISemanticNode>> _semanticFactoryBySyntaxType = 
            new Dictionary<Type, Func<SyntaxNode, ISemanticNode>>() {
                [typeof(ValueSyntax)] = FuncOfSyntax<ValueSyntax>(NodeFactory.CreateAnyValue),
                [typeof(ScalarValueSyntax)] = FuncOfSyntax<ScalarValueSyntax>(NodeFactory.CreateScalarValue),
                [typeof(ObjectValueSyntax)] = FuncOfSyntax<ObjectValueSyntax>(NodeFactory.CreateObjectValue),
                [typeof(ArrayValueSyntax)] = FuncOfSyntax<ArrayValueSyntax>(NodeFactory.CreateArrayValue),
                [typeof(ObjectSyntax)] = FuncOfSyntax<ObjectSyntax>(NodeFactory.CreateObject),
                [typeof(ArraySyntax)] = FuncOfSyntax<ArraySyntax>(NodeFactory.CreateArray),
            };  

        public static ISemanticNode CreateFromSyntax(SyntaxNode syntax)
        {
            var factory = _semanticFactoryBySyntaxType[syntax.GetType()];
            var semanticNode = factory(syntax);
            return semanticNode;
        }

        public interface ISemanticNode
        {
        }

        public abstract class ValueNode : ISemanticNode
        {
        }

        public class StringValueNode : ValueNode
        {
            public StringValueNode(string value)
            {
                Value = value;
            }

            public string Value { get; }
        }

        public class NumberValueNode : ValueNode
        {
            public NumberValueNode(decimal value)
            {
                Value = value;
            }

            public decimal Value { get; set; }
        }

        public class BooleanValueNode : ValueNode
        {
            public BooleanValueNode(bool value)
            {
                Value = value;
            }

            public bool Value { get; set; }
        }

        public class ObjectValueNode : ValueNode
        {
            public ObjectValueNode(ObjectNode value)
            {
                Value = value;
            }

            public ObjectNode Value { get; set; }
        }

        public class ArrayValueNode : ValueNode
        {
            public ArrayValueNode(ArrayNode value)
            {
                Value = value;
            }

            public ArrayNode Value { get; set; }
        }

        public class ObjectNode : ISemanticNode
        {
            public ObjectNode(IEnumerable<PropertyNode> properties)
            {
                Properties = properties.ToList();
            }

            public List<PropertyNode> Properties { get; }
        }

        public class ArrayNode : ISemanticNode
        {
            public ArrayNode(IEnumerable<ValueNode> items)
            {
                Items = items.ToList();
            }

            public List<ValueNode> Items { get; }
        }

        public class PropertyNode : ISemanticNode
        {
            public PropertyNode(string name, ValueNode value)
            {
                Name = name;
                Value = value;
            }

            public string Name { get; }
            public ValueNode Value { get; }
        }   

        private static Func<SyntaxNode, ISemanticNode> FuncOfSyntax<TSyntax>(Func<TSyntax, ISemanticNode> func)
            where TSyntax : SyntaxNode
        {
            return syntax => func((TSyntax)syntax);
        }

        private static class NodeFactory
        {
            public static ValueNode CreateAnyValue(ValueSyntax syntax) 
            {
                return syntax switch 
                {
                    ScalarValueSyntax scalar => CreateScalarValue(scalar),
                    ObjectValueSyntax obj => CreateObjectValue(obj),
                    ArrayValueSyntax arr => CreateArrayValue(arr),
                    _ => throw new Exception("Unrecognized syntax node.")
                };
            }

            public static ValueNode CreateScalarValue(ScalarValueSyntax syntax) 
            {
                return syntax.ScalarToken.ClrValue switch {
                    string s => new StringValueNode(s),
                    decimal n => new NumberValueNode(n),
                    bool b => new BooleanValueNode(b),
                    _ => throw new Exception("Unrecognized scalar value token")
                };
            }

            public static ObjectValueNode CreateObjectValue(ObjectValueSyntax syntax) 
            {
                return new ObjectValueNode(CreateObject(syntax.ObjectSyntax));
            }

            public static ArrayValueNode CreateArrayValue(ArrayValueSyntax syntax) 
            {
                return new ArrayValueNode(CreateArray(syntax.ArraySyntax));
            }

            public static ObjectNode CreateObject(ObjectSyntax syntax)
            {
                return new ObjectNode(syntax.Properties.Select(CreateProperty));
            }

            public static ArrayNode CreateArray(ArraySyntax syntax)
            {
                return new ArrayNode(syntax.Items.Select(CreateAnyValue));
            }

            private static PropertyNode CreateProperty(PropertySyntax syntax)
            {
                return new PropertyNode(syntax.Name, CreateAnyValue(syntax.ValueSyntax));
            }
        }
    }
}