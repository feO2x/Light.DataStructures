using System;
using FluentAssertions;
using Xunit;

namespace Light.DataStructures.Tests
{
    [Trait("Category", Traits.FunctionalTests)]
    public sealed class EntryTests
    {
        [Theory]
        [InlineData(42, 42, "Foo")]
        [InlineData(-2, "Bar", "Baz")]
        [InlineData(int.MinValue / 2, 44.0002, true)]
        public void ParametersMustBeRetrievable<TKey, TValue>(int hashCode, TKey key, TValue value)
        {
            var entry = new Entry<TKey, TValue>(hashCode, key, value);

            entry.HashCode.Should().Be(hashCode);
            entry.Key.Should().Be(key);
            entry.Value.Should().Be(value);
        }

        [Fact]
        public void KeyNotNull()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Action act = () => new Entry<string, object>(0, null, new object());

            act.ShouldThrow<ArgumentNullException>()
               .And.ParamName.Should().Be("key");
        }
    }
}