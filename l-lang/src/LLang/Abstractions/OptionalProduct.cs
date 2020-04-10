using System;

namespace LLang.Abstractions
{
    public readonly struct OptionalProduct<T>
    {
        private readonly bool _hasValue;
        private readonly T _value;

        public OptionalProduct(bool hasValue, T value = default)
        {
            _hasValue = hasValue;
            _value = value;
        }

        public override string ToString()
        {
            return $"OptProd{{{(_hasValue ? _value?.ToString() : "N/A")}}}";
        }

        public bool HasValue => _hasValue;
        public T Value => _hasValue 
            ? _value 
            : throw new InvalidOperationException($"{nameof(OptionalProduct<T>)} has no value");
    }

    public static class OptionalProduct
    {
        public static OptionalProduct<T> WithValue<T>(T product)
        {
            return new OptionalProduct<T>(hasValue: true, value: product);
        }

        public static OptionalProduct<T> WithoutValue<T>()
        {
            return new OptionalProduct<T>(hasValue: false);
        }
    }
}
