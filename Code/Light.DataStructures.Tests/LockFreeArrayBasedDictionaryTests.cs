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
        public void KickOff()
        {
            var dictionary = new LockFreeArrayBasedDictionary<string, string> { { "Foo", "Bar" } };

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
                new object[] {new Action<LockFreeArrayBasedDictionary<int, object>>(testTarget => testTarget.Add(5, null)), 5, true},
                new object[] {new Action<LockFreeArrayBasedDictionary<int, object>>(testTarget => testTarget.Add(14, null)), 14, true},
                new object[] {new Action<LockFreeArrayBasedDictionary<int, object>>(testTarget => testTarget.Add(7, null)), 3, false},
                new object[] {new Action<LockFreeArrayBasedDictionary<int, object>>(testTarget => testTarget.Add(42, null)), 1, false}
            };
    }
}