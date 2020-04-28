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

        public override bool Equals(object? other)
        {
            return other is Marker<TIn> otherMarker && this.Value == otherMarker.Value;
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

        public static Marker<TIn> operator - (Marker<TIn> left, int right) {
            return new Marker<TIn>(left.Value - right);
        }

        public static Marker<TIn> operator + (Marker<TIn> left, int right) {
            return new Marker<TIn>(left.Value + right);
        }
    }
}
