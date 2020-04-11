using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using LLang.Abstractions.Languages;
using static LLang.Demos.Json.JsonGrammar;

namespace LLang.Demos.Json
{
    public class JsonSemantics
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

        [DataContract]
        [KnownType(typeof(StringValueNode))]
        [KnownType(typeof(NumberValueNode))]
        [KnownType(typeof(BooleanValueNode))]
        [KnownType(typeof(ObjectValueNode))]
        [KnownType(typeof(ArrayValueNode))]
        public abstract class ValueNode : ISemanticNode
        {
        }

        [DataContract]
        public class StringValueNode : ValueNode
        {
            public StringValueNode(string value)
            {
                Value = value;
            }

            [DataMember]
            public string Value { get; set;}
        }

        [DataContract]
        public class NumberValueNode : ValueNode
        {
            public NumberValueNode(decimal value)
            {
                Value = value;
            }

            [DataMember]
            public decimal Value { get; set; }
        }

        [DataContract]
        public class BooleanValueNode : ValueNode
        {
            public BooleanValueNode(bool value)
            {
                Value = value;
            }

            [DataMember]
            public bool Value { get; set; }
        }

        [DataContract]
        public class ObjectValueNode : ValueNode
        {
            public ObjectValueNode(ObjectNode value)
            {
                Value = value;
            }

            [DataMember]
            public ObjectNode Value { get; set; }
        }

        [DataContract]
        public class ArrayValueNode : ValueNode
        {
            public ArrayValueNode(ArrayNode value)
            {
                Value = value;
            }

            [DataMember]
            public ArrayNode Value { get; set; }
        }

        [DataContract]
        public class ObjectNode : ISemanticNode
        {
            public ObjectNode(IEnumerable<PropertyNode> properties)
            {
                Properties = properties.ToList();
            }

            [DataMember]
            public List<PropertyNode> Properties { get; set; }
        }

        [DataContract]
        public class ArrayNode : ISemanticNode
        {
            public ArrayNode(IEnumerable<ValueNode> items)
            {
                Items = items.ToList();
            }

            [DataMember]
            public List<ValueNode> Items { get; set; }
        }

        [DataContract]
        public class PropertyNode : ISemanticNode
        {
            public PropertyNode(string name, ValueNode value)
            {
                Name = name;
                Value = value;
            }

            [DataMember]
            public string Name { get; set; }
            [DataMember]
            public ValueNode Value { get; set; }
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