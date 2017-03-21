using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Light.DataStructures.PrecompiledDictionaryServices;
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

        [Fact]
        public void DicionaryShouldImplementIDictionary()
        {
            typeof(PrecompiledDictionary<string, int>).Should().Implement<IDictionary<string, int>>();
        }

        [Theory]
        [MemberData(nameof(DifferentHashCodesData))]
        public void RetrieveItemsWithDifferentHashCodes(KeyValuePair<int, object>[] keyValuePairs)
        {
            var dictionary = CreateTestTarget(keyValuePairs);

            foreach (var keyValuePair in keyValuePairs)
            {
                dictionary[keyValuePair.Key].Should().BeSameAs(keyValuePair.Value);
            }
        }

        [Theory]
        [MemberData(nameof(DifferentHashCodesData))]
        public void CountMustBeSameAsNumberOfKvps(KeyValuePair<int, object>[] keyValuePairs)
        {
            var dictionary = CreateTestTarget(keyValuePairs);

            dictionary.Count.Should().Be(keyValuePairs.Length);
        }

        [Theory]
        [MemberData(nameof(DifferentHashCodesData))]
        public void KeysMustBeAccessible(KeyValuePair<int, object>[] keyValuePairs)
        {
            var dictionary = CreateTestTarget(keyValuePairs);

            dictionary.Keys.ShouldAllBeEquivalentTo(keyValuePairs.Select(kvp => kvp.Key));
        }

        [Theory]
        [MemberData(nameof(DifferentHashCodesData))]
        public void ValuesMustBeAccessible(KeyValuePair<int, object>[] keyValuePairs)
        {
            var dictionary = CreateTestTarget(keyValuePairs);

            dictionary.Values.ShouldAllBeEquivalentTo(keyValuePairs.Select(kvp => kvp.Value));
        }

        public static readonly TestData DifferentHashCodesData =
            new[]
            {
                new object[] { new[] { new KeyValuePair<int, object>(42, new object()) } },
                new object[] { new[] { new KeyValuePair<int, object>(1, new object()), new KeyValuePair<int, object>(2, new object()) } },
                new object[] { new[] { new KeyValuePair<int, object>(-44, new object()), new KeyValuePair<int, object>(1995, new object()), new KeyValuePair<int, object>(-585854, new object()) } }
            };

        [Theory]
        [MemberData(nameof(ContainsKeyData))]
        public void ContainsKey(KeyValuePair<string, object>[] keyValuePairs, string requestedKey, bool expected)
        {
            var dictionary = CreateTestTarget(keyValuePairs);

            var actualResult = dictionary.ContainsKey(requestedKey);

            actualResult.Should().Be(expected);
        }

        [Theory]
        [MemberData(nameof(DifferentHashCodesData))]
        public void GetEnumerator(KeyValuePair<int, object>[] keyValuePairs)
        {
            var dictionary = CreateTestTarget(keyValuePairs);

            dictionary.ShouldAllBeEquivalentTo(keyValuePairs);
        }

        public static readonly TestData ContainsKeyData =
            new[]
            {
                new object[]
                {
                    new[]
                    {
                        new KeyValuePair<string, object>("Foo", new object()),
                        new KeyValuePair<string, object>("Bar", new object())
                    },
                    "Foo",
                    true
                },
                new object[]
                {
                    new[]
                    {
                        new KeyValuePair<string, object>("Foo", new object()),
                        new KeyValuePair<string, object>("Bar", new object())
                    },
                    "Bar",
                    true
                },
                new object[]
                {
                    new[]
                    {
                        new KeyValuePair<string, object>("Foo", new object()),
                        new KeyValuePair<string, object>("Bar", new object()),
                        new KeyValuePair<string, object>("Baz", new object()),
                        new KeyValuePair<string, object>("Qux", new object())
                    },
                    "Quux",
                    false
                }
            };

        [Theory]
        [MemberData(nameof(TryGetValueData))]
        public void TryGetValue(KeyValuePair<int, string>[] keyValuePairs, int key, bool expectedResult, string expectedValue)
        {
            var dictionary = CreateTestTarget(keyValuePairs);

            var actualResult = dictionary.TryGetValue(key, out var actualValue);

            actualResult.Should().Be(expectedResult);
            actualValue.Should().Be(expectedValue);
        }

        public static readonly TestData TryGetValueData =
            new[]
            {
                new object[]
                {
                    new[]
                    {
                        new KeyValuePair<int, string>(42, "Foo"),
                        new KeyValuePair<int, string>(43, "Bar")
                    },
                    43,
                    true,
                    "Bar"
                },
                new object[]
                {
                    new[]
                    {
                        new KeyValuePair<int, string>(-1, "Foo"),
                        new KeyValuePair<int, string>(2, "Bar"),
                        new KeyValuePair<int, string>(-8, "Baz")
                    },
                    5,
                    false,
                    null
                }
            };

        [Fact]
        public void IsReadOnlyMustAlwaysBeTrue()
        {
            CreateTestTarget(new KeyValuePair<int, string>(42, "Foo")).IsReadOnly.Should().BeTrue();
        }

        public static PrecompiledDictionary<TKey, TValue> CreateTestTarget<TKey, TValue>(params KeyValuePair<TKey, TValue>[] keyValuePairs)
        {
            return new PrecompiledDictionaryFactory(new DefaultLookupFunctionCompiler()).Create(keyValuePairs);
        }
    }
}