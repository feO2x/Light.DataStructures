using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Light.DataStructures.Tests
{
    [Trait("Category", Traits.FunctionalTests)]
    public sealed class PrecompiledDictionaryTests
    {
        [Fact]
        public void DictionaryShouldImplementIReadOnlyDictionary()
        {
            typeof(PrecompiledDictionary<string, string>).Should().Implement<IReadOnlyDictionary<string, string>>();
        }

        [Fact]
        public void RetrieveSingleExistingItem()
        {
            var entry = new KeyValuePair<int, object>(42, new object());
            var dictionary = PrecompiledDictionary.CreateFrom(entry);

            dictionary[42].Should().BeSameAs(entry.Value);
        }
    }
}