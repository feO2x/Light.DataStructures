using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using TestData = System.Collections.Generic.IEnumerable<object[]>;

namespace Light.DataStructures.Tests
{
    public sealed class LockFreeArrayBasedDictionaryTests
    {
        [Fact]
        public void AddAndRetrieve()
        {
            var dictionary = new LockFreeArrayBasedDictionary<string, string>();

            var result = dictionary.Add("Foo", "Bar");

            dictionary["Foo"].Should().Be("Bar");
            result.Should().BeSameAs(dictionary);
        }

        [Fact]
        public void ImplementsIDictionary()
        {
            typeof(LockFreeArrayBasedDictionary<string, object>).Should().Implement<IDictionary<string, object>>();
        }

        [Fact]
        public void IsNotReadonly()
        {
            new LockFreeArrayBasedDictionary<string, object>().IsReadOnly.Should().BeFalse();
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
                new object[] { new Action<LockFreeArrayBasedDictionary<int, object>>(testTarget => testTarget.Add(5, null)), 5, true },
                new object[] { new Action<LockFreeArrayBasedDictionary<int, object>>(testTarget => testTarget.Add(14, null)), 14, true },
                new object[] { new Action<LockFreeArrayBasedDictionary<int, object>>(testTarget => testTarget.Add(7, null)), 3, false },
                new object[] { new Action<LockFreeArrayBasedDictionary<int, object>>(testTarget => testTarget.Add(42, null)), 1, false }
            };

        [Theory]
        [MemberData(nameof(ItemsToAddData))]
        public void CountMustReflectNumberOfAddedItems(KeyValuePair<string, object>[] itemsToAdd)
        {
            var dictionary = new LockFreeArrayBasedDictionary<string, object>();
            LockFreeArrayBasedDictionary<string, object> result = null;
            foreach (var keyValuePair in itemsToAdd)
            {
                result = dictionary.Add(keyValuePair);
            }

            dictionary.Count.Should().Be(itemsToAdd.Length);
            result.Should().BeSameAs(dictionary);
        }

        [Theory]
        [MemberData(nameof(ItemsToAddData))]
        public void Clear(KeyValuePair<string, object>[] itemsToAdd)
        {
            var dictionary = new LockFreeArrayBasedDictionary<string, object>();
            foreach (var keyValuePair in itemsToAdd)
            {
                dictionary.Add(keyValuePair);
            }

            var result = dictionary.Clear();

            dictionary.Count.Should().Be(0);
            result.Should().BeTrue();
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
    }
}