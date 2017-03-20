﻿using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using TestData = System.Collections.Generic.IEnumerable<object[]>;

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

        [Theory]
        [MemberData(nameof(DifferentHashCodesData))]
        public void RetrieveItemsWithDifferentHashCodes(KeyValuePair<int, object>[] keyValuePairs)
        {
            var dictionary = PrecompiledDictionary.CreateFrom(keyValuePairs);

            foreach (var keyValuePair in keyValuePairs)
            {
                dictionary[keyValuePair.Key].Should().BeSameAs(keyValuePair.Value);
            }
        }

        [Theory]
        [MemberData(nameof(DifferentHashCodesData))]
        public void CountMustBeSameAsNumberOfKvps(KeyValuePair<int, object>[] keyValuePairs)
        {
            var dictionary = PrecompiledDictionary.CreateFrom(keyValuePairs);

            dictionary.Count.Should().Be(keyValuePairs.Length);
        }

        public static readonly TestData DifferentHashCodesData =
            new[]
            {
                new object[] { new[] { new KeyValuePair<int, object>(42, new object()) } },
                new object[] { new[] { new KeyValuePair<int, object>(1, new object()), new KeyValuePair<int, object>(2, new object()) } },
                new object[] { new[] { new KeyValuePair<int, object>(-44, new object()), new KeyValuePair<int, object>(1995, new object()), new KeyValuePair<int, object>(-585854, new object()) } }
            };
    }
}