using System;
using FluentAssertions;

namespace LLang.Tests
{
    public static class PropDrillingExtensions
    {
        public static T DrillAs<T>(this object? value, Action<T>? assert = null, string? because = null)
            where T : class
        {
            value.Should().NotBeNull(because);
            value.Should().BeOfType<T>(because);
            T drilled = (T)value!;
            assert?.Invoke(drilled);
            return drilled;
        }

        public static T Drill<T>(this T value, Action<T> assert)
        {
            assert(value);
            return value;
        }
    }
}
