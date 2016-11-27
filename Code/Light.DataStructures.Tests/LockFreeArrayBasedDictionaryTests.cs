using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using TestData = System.Collections.Generic.IEnumerable<object[]>;
using System.Linq;

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
                        new KeyValuePair<string, object>("Baz", null),
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
                        new KeyValuePair<string, object>("Quux", null),
                    }
                },
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
            Action act = () => { var value = dictionary[null]; };

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
                    new LockFreeArrayBasedDictionary<int, string> {[42] = "Foo"},
                    new KeyValuePair<int, string>(42, "Foo"),
                    true
                },
                new object[]
                {
                    new LockFreeArrayBasedDictionary<int, string> {[42] = "Foo"},
                    new KeyValuePair<int, string>(43, "Foo"),
                    false
                },
                new object[]
                {
                    new LockFreeArrayBasedDictionary<int, string> {[42] = "Foo"},
                    new KeyValuePair<int, string>(42, "Bar"),
                    false
                },
                new object[]
                {
                    new LockFreeArrayBasedDictionary<int, string> {[1] = "A", [2] = "B", [3] = "A"},
                    new KeyValuePair<int, string>(3, "A"),
                    true
                },
                new object[]
                {
                    new LockFreeArrayBasedDictionary<int, string> {[1] = "A", [2] = "B", [3] = "A"},
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

        [Theory]
        [InlineData(0.3f)]
        [InlineData(0.55f)]
        [InlineData(0.9f)]
        public void LoadThresholdCanBeChanged(float newThreshold)
        {
            var dictionary = new LockFreeArrayBasedDictionary<string, object> { LoadThreshold = newThreshold };

            dictionary.LoadThreshold.Should().Be(newThreshold);
        }

        [Fact]
        public void ImplementsIConcurrentDictionary()
        {
            typeof(LockFreeArrayBasedDictionary<string, object>).Should().Implement<IConcurrentDictionary<string, object>>();
        }
    }
}