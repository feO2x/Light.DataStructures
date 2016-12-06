using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;
using TestData = System.Collections.Generic.IEnumerable<object[]>;

namespace Light.DataStructures.Tests
{
    [Trait("Category", Traits.FunctionalTests)]
    public sealed class LockFreeArrayBasedDictionaryTests
    {
        [Fact]
        public void AddAndRetrieve()
        {
            var dictionary = new LockFreeArrayBasedDictionary<string, string>();

            var result = dictionary.TryAdd("Foo", "Bar");

            result.Should().BeTrue();
            dictionary["Foo"].Should().Be("Bar");
        }

        [Fact]
        public void ImplementsIDictionary()
        {
            typeof(LockFreeArrayBasedDictionary<string, object>).Should().Implement<IDictionary<string, object>>();
        }

        [Fact]
        public void IsNotReadonly()
        {
            // ReSharper disable once CollectionNeverUpdated.Local
            IDictionary<string, object> dictionary = new LockFreeArrayBasedDictionary<string, object>();

            dictionary.IsReadOnly.Should().BeFalse();
        }

        [Theory]
        [MemberData(nameof(ContainsKeyData))]
        public void ContainsKey(Action<LockFreeArrayBasedDictionary<int, object>> configureTestTarget, int key, bool expected)
        {
            var testTarget = new LockFreeArrayBasedDictionary<int, object>();
            configureTestTarget(testTarget);

            var actual = testTarget.ContainsKey(key);

            actual.Should().Be(expected);
        }

        public static readonly TestData ContainsKeyData =
            new[]
            {
                new object[] { new Action<LockFreeArrayBasedDictionary<int, object>>(testTarget => testTarget.TryAdd(5, null)), 5, true },
                new object[] { new Action<LockFreeArrayBasedDictionary<int, object>>(testTarget => testTarget.TryAdd(14, null)), 14, true },
                new object[] { new Action<LockFreeArrayBasedDictionary<int, object>>(testTarget => testTarget.TryAdd(7, null)), 3, false },
                new object[] { new Action<LockFreeArrayBasedDictionary<int, object>>(testTarget => testTarget.TryAdd(42, null)), 1, false }
            };

        [Theory]
        [MemberData(nameof(ItemsToAddData))]
        public void CountMustReflectNumberOfAddedItems(KeyValuePair<string, object>[] itemsToAdd)
        {
            IDictionary<string, object> dictionary = new LockFreeArrayBasedDictionary<string, object>();
            foreach (var keyValuePair in itemsToAdd)
            {
                dictionary.Add(keyValuePair);
            }

            dictionary.Count.Should().Be(itemsToAdd.Length);
        }

        [Theory]
        [MemberData(nameof(ItemsToAddData))]
        public void Clear(KeyValuePair<string, object>[] itemsToAdd)
        {
            IDictionary<string, object> dictionary = new LockFreeArrayBasedDictionary<string, object>();
            foreach (var keyValuePair in itemsToAdd)
            {
                dictionary.Add(keyValuePair);
            }

            dictionary.Clear();

            dictionary.Count.Should().Be(0);
        }

        [Theory]
        [MemberData(nameof(ItemsToAddData))]
        public void ClearViaInterface(KeyValuePair<string, object>[] itemsToAdd)
        {
            IDictionary<string, object> dictionary = new LockFreeArrayBasedDictionary<string, object>();
            foreach (var keyValuePair in itemsToAdd)
            {
                dictionary.Add(keyValuePair);
            }

            dictionary.Clear();

            dictionary.Count.Should().Be(0);
        }

        [Theory]
        [MemberData(nameof(ItemsToAddData))]
        public void AddViaInterface(KeyValuePair<string, object>[] itemsToAdd)
        {
            IDictionary<string, object> dictionary = new LockFreeArrayBasedDictionary<string, object>();

            foreach (var keyValuePair in itemsToAdd)
            {
                dictionary.Add(keyValuePair.Key, keyValuePair.Value);
            }

            dictionary.Count.Should().Be(itemsToAdd.Length);
        }

        public static readonly TestData ItemsToAddData =
            new[]
            {
                new object[]
                {
                    new[]
                    {
                        new KeyValuePair<string, object>("Foo", null)
                    }
                },
                new object[]
                {
                    new[]
                    {
                        new KeyValuePair<string, object>("Foo", null),
                        new KeyValuePair<string, object>("Bar", null),
                        new KeyValuePair<string, object>("Baz", null)
                    }
                },
                new object[]
                {
                    new[]
                    {
                        new KeyValuePair<string, object>("Foo", null),
                        new KeyValuePair<string, object>("Bar", null),
                        new KeyValuePair<string, object>("Baz", null),
                        new KeyValuePair<string, object>("Qux", null),
                        new KeyValuePair<string, object>("Quux", null)
                    }
                }
            };

        [Fact]
        public void IndexerSetKeyNull()
        {
            // ReSharper disable once CollectionNeverQueried.Local
            var dictionary = new LockFreeArrayBasedDictionary<string, object>();

            // ReSharper disable once AssignNullToNotNullAttribute
            Action act = () => dictionary[null] = new object();

            act.ShouldThrow<ArgumentNullException>()
               .And.ParamName.Should().Be("key");
        }

        [Fact]
        public void IndexerGetKeyNull()
        {
            // ReSharper disable once CollectionNeverUpdated.Local
            var dictionary = new LockFreeArrayBasedDictionary<string, object>();

            // ReSharper disable once UnusedVariable
            Action act = () =>
                         {
                             var value = dictionary[null];
                         };

            act.ShouldThrow<ArgumentNullException>()
               .And.ParamName.Should().Be("key");
        }

        [Theory]
        [MemberData(nameof(ContainsData))]
        public void Contains(LockFreeArrayBasedDictionary<int, string> dictionary, KeyValuePair<int, string> targetPair, bool expected)
        {
            var result = dictionary.Contains(targetPair);

            result.Should().Be(expected);
        }

        public static readonly TestData ContainsData =
            new[]
            {
                new object[]
                {
                    new LockFreeArrayBasedDictionary<int, string> { [42] = "Foo" },
                    new KeyValuePair<int, string>(42, "Foo"),
                    true
                },
                new object[]
                {
                    new LockFreeArrayBasedDictionary<int, string> { [42] = "Foo" },
                    new KeyValuePair<int, string>(43, "Foo"),
                    false
                },
                new object[]
                {
                    new LockFreeArrayBasedDictionary<int, string> { [42] = "Foo" },
                    new KeyValuePair<int, string>(42, "Bar"),
                    false
                },
                new object[]
                {
                    new LockFreeArrayBasedDictionary<int, string> { [1] = "A", [2] = "B", [3] = "A" },
                    new KeyValuePair<int, string>(3, "A"),
                    true
                },
                new object[]
                {
                    new LockFreeArrayBasedDictionary<int, string> { [1] = "A", [2] = "B", [3] = "A" },
                    new KeyValuePair<int, string>(58, "X"),
                    false
                }
            };

        [Theory]
        [InlineData(50)]
        [InlineData(100)]
        public void IncreaseCapacity(int numberOfItems)
        {
            var keys = Enumerable.Range(1, numberOfItems).ToArray();
            var dictionary = new LockFreeArrayBasedDictionary<int, object>();

            foreach (var key in keys)
            {
                dictionary.TryAdd(key, new object());
            }

            dictionary.Should().ContainKeys(keys);
        }

        [Fact]
        public void ImplementsIConcurrentDictionary()
        {
            typeof(LockFreeArrayBasedDictionary<string, object>).Should().Implement<IConcurrentDictionary<string, object>>();
        }

        [Theory]
        [InlineData("Foo", "Bar")]
        [InlineData(42, new[] { "1", "2", "3" })]
        public void AddOrUpdateNonExistingEntry<TKey, TValue>(TKey key, TValue value)
        {
            var dictionary = new LockFreeArrayBasedDictionary<TKey, TValue>();

            var wasAdded = dictionary.AddOrUpdate(key, value);

            wasAdded.Should().BeTrue();
            dictionary.Should().Contain(key, value);
        }

        [Theory]
        [InlineData("Foo", "Bar", "Baz")]
        [InlineData("qux", 1, 2)]
        public void AddOrUpdateExistingEntry<TKey, TValue>(TKey key, TValue value1, TValue value2)
        {
            var dictionary = new LockFreeArrayBasedDictionary<TKey, TValue>();
            dictionary.TryAdd(key, value1);

            var wasAdded = dictionary.AddOrUpdate(key, value2);

            wasAdded.Should().BeFalse();
            dictionary.Should().Contain(key, value2);
            dictionary.Should().NotContain(key, value1);
        }

        [Theory]
        [MemberData(nameof(SuccessfulTryGetValueData))]
        public void SuccessfulTryGetValue<TKey, TValue>(TKey searchedKey, TValue expectedValue, IEnumerable<KeyValuePair<TKey, TValue>> existingEntries)
        {
            var dictionary = new LockFreeArrayBasedDictionary<TKey, TValue>();
            foreach (var existingEntry in existingEntries)
            {
                dictionary.TryAdd(existingEntry.Key, existingEntry.Value);
            }

            TValue foundValue;
            var wasFound = dictionary.TryGetValue(searchedKey, out foundValue);

            wasFound.Should().BeTrue();
            foundValue.Should().Be(expectedValue);
        }

        public static readonly TestData SuccessfulTryGetValueData =
            new[]
            {
                new object[]
                {
                    42,
                    "Foo",
                    new[]
                    {
                        new KeyValuePair<int, string>(42, "Foo"),
                        new KeyValuePair<int, string>(43, "Bar"),
                        new KeyValuePair<int, string>(44, "Baz")
                    }
                },
                new object[]
                {
                    "Bar",
                    false,
                    new[]
                    {
                        new KeyValuePair<string, bool>("Foo", true),
                        new KeyValuePair<string, bool>("Bar", false)
                    }
                }
            };

        [Theory]
        [MemberData(nameof(UnsuccessfulTryGetValueData))]
        public void UnsuccessfulTryGetValue<TKey>(TKey searchedKey, IEnumerable<KeyValuePair<TKey, object>> existingEntries)
        {
            var dictionary = new LockFreeArrayBasedDictionary<TKey, object>();
            foreach (var existingEntry in existingEntries)
            {
                dictionary.TryAdd(existingEntry.Key, existingEntry.Value);
            }

            object foundValue;
            var wasFound = dictionary.TryGetValue(searchedKey, out foundValue);

            wasFound.Should().BeFalse();
            foundValue.Should().Be(default(object));
        }

        public static readonly TestData UnsuccessfulTryGetValueData =
            new[]
            {
                new object[]
                {
                    19992,
                    new[]
                    {
                        new KeyValuePair<int, object>(1, new object()),
                        new KeyValuePair<int, object>(2, new object())
                    }
                },
                new object[]
                {
                    "thud",
                    new[]
                    {
                        new KeyValuePair<string, object>("Foo", new object()),
                        new KeyValuePair<string, object>("Bar", new object()),
                        new KeyValuePair<string, object>("Baz", new object())
                    }
                }
            };

        [Theory]
        [InlineData(42, "Foo")]
        [InlineData("Bar", new object[] { })]
        public void RemoveExisting<TKey, TValue>(TKey key, TValue value)
        {
            var dictionary = new LockFreeArrayBasedDictionary<TKey, TValue>();
            dictionary.TryAdd(key, value);

            var wasRemoved = dictionary.Remove(key);

            wasRemoved.Should().BeTrue();
            dictionary.Should().NotContainKey(key);
        }

        [Theory]
        [MemberData(nameof(RemoveNonExistingData))]
        public void RemoveNonExisting(string key, KeyValuePair<string, object>[] existingEntries)
        {
            var dictionary = new LockFreeArrayBasedDictionary<string, object>();
            foreach (var existingEntry in existingEntries)
            {
                dictionary.TryAdd(existingEntry.Key, existingEntry.Value);
            }

            var wasRemoved = dictionary.Remove(key);

            wasRemoved.Should().Be(false);
            dictionary.Should().NotContainKey(key);
        }

        public static readonly TestData RemoveNonExistingData =
            new[]
            {
                new object[]
                {
                    "Qux",
                    new[]
                    {
                        new KeyValuePair<string, object>("Foo", new object()),
                        new KeyValuePair<string, object>("Bar", new object()),
                        new KeyValuePair<string, object>("Baz", new object())
                    }
                },
                new object[]
                {
                    "thud",
                    new[]
                    {
                        new KeyValuePair<string, object>("1", "Foo"),
                        new KeyValuePair<string, object>("2", "Bar"),
                        new KeyValuePair<string, object>("3", "Baz"),
                        new KeyValuePair<string, object>("4", "Qux")
                    }
                }
            };
    }
}