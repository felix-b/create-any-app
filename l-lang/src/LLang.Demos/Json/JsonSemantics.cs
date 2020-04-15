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
        private static readonly Dictionary<Type, Func<SyntaxNode, object?>> _semanticFactoryBySyntaxType = 
            new Dictionary<Type, Func<SyntaxNode, object?>>() {
                [typeof(ValueSyntax)] = FuncOfSyntax<ValueSyntax>(NodeFactory.CreateAnyValue),
                [typeof(ScalarValueSyntax)] = FuncOfSyntax<ScalarValueSyntax>(NodeFactory.CreateScalarValue),
                [typeof(ObjectValueSyntax)] = FuncOfSyntax<ObjectValueSyntax>(NodeFactory.CreateObjectValue),
                [typeof(ArrayValueSyntax)] = FuncOfSyntax<ArrayValueSyntax>(NodeFactory.CreateArrayValue),
                [typeof(ObjectSyntax)] = FuncOfSyntax<ObjectSyntax>(NodeFactory.CreateObject),
                [typeof(ArraySyntax)] = FuncOfSyntax<ArraySyntax>(NodeFactory.CreateArray),
            };  

        public static object? CreateFromSyntax(SyntaxNode syntax)
        {
            var factory = _semanticFactoryBySyntaxType[syntax.GetType()];
            var semanticNode = factory(syntax);
            return semanticNode;
        }

        [DataContract]
        [KnownType(typeof(ObjectNode))]
        [KnownType(typeof(ArrayNode))]
        public class JsonNode
        {
        }

        [DataContract]
        [KnownType(typeof(ObjectNode))]
        [KnownType(typeof(ArrayNode))]
        public class ObjectNode : JsonNode
        {
            public ObjectNode(IEnumerable<PropertyNode> properties)
            {
                Properties = properties.ToList();
            }

            [DataMember]
            public List<PropertyNode> Properties { get; set; }
        }

        [DataContract]
        [KnownType(typeof(ObjectNode))]
        [KnownType(typeof(ArrayNode))]
        public class ArrayNode : JsonNode
        {
            public ArrayNode(IEnumerable<object?> items)
            {
                Items = items.ToList();
            }

            [DataMember]
            public List<object?> Items { get; set; }
        }

        [DataContract]
        [KnownType(typeof(ObjectNode))]
        [KnownType(typeof(ArrayNode))]
        public class PropertyNode : JsonNode
        {
            public PropertyNode(string name, object? value)
            {
                Name = name;
                Value = value;
            }

            [DataMember]
            public string Name { get; set; }
            [DataMember]
            public object? Value { get; set; }
        }   

        private static Func<SyntaxNode, object?> FuncOfSyntax<TSyntax>(Func<TSyntax, object?> func)
            where TSyntax : SyntaxNode
        {
            return syntax => func((TSyntax)syntax);
        }

        private static class NodeFactory
        {
            public static object? CreateAnyValue(ValueSyntax syntax) 
            {
                return syntax switch 
                {
                    ScalarValueSyntax scalar => CreateScalarValue(scalar),
                    ObjectValueSyntax obj => CreateObjectValue(obj),
                    ArrayValueSyntax arr => CreateArrayValue(arr),
                    _ => throw new Exception("Unrecognized syntax node.")
                };
            }

            public static object? CreateScalarValue(ScalarValueSyntax syntax) 
            {
                return syntax.ScalarToken.ClrValue;
            }

            public static ObjectNode CreateObjectValue(ObjectValueSyntax syntax) 
            {
                return CreateObject(syntax.ObjectSyntax);
            }

            public static ArrayNode CreateArrayValue(ArrayValueSyntax syntax) 
            {
                return CreateArray(syntax.ArraySyntax);
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