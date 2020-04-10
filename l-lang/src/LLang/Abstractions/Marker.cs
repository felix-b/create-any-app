using System;

namespace LLang.Abstractions
{
    public readonly struct Marker<TIn>
    {
        public readonly int Value;

        public Marker(int value)
        {
            Value = value;
        }

        public override bool Equals(object? obj)
        {
            return obj is Marker<TIn> marker && Value == marker.Value;
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public static bool operator == (Marker<TIn> left, Marker<TIn> right) {
            return left.Value == right.Value;
        }

        public static bool operator != (Marker<TIn> left, Marker<TIn> right) {
            return left.Value != right.Value;
        }

        public static bool operator > (Marker<TIn> left, Marker<TIn> right) {
            return left.Value > right.Value;
        }

        public static bool operator < (Marker<TIn> left, Marker<TIn> right) {
            return left.Value < right.Value;
        }

        public static int operator - (Marker<TIn> left, Marker<TIn> right) {
            return left.Value - right.Value;
        }
    }
}
