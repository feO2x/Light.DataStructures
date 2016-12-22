using System;

namespace Light.DataStructures
{
    /// <summary>
    ///     Represents a dictionary that can be accessed by several threads simultaneously.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    public interface IConcurrentDictionary<in TKey, TValue>
    {
        /// <summary>
        ///     Attempts to add the specified key and value to the <see cref="IConcurrentDictionary{TKey, TValue}" />.
        /// </summary>
        /// <param name="key"> The key of the element to add.</param>
        /// <param name="value">
        ///     The value of the element to add. The value can be a null reference (Nothing in Visual Basic) for reference types.
        /// </param>
        /// <returns>
        ///     true if the key/value pair was added to the <see cref="IConcurrentDictionary{TKey, TValue}" /> successfully; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="key" /> is null reference (Nothing in Visual Basic). </exception>
        bool TryAdd(TKey key, TValue value);

        /// <summary>
        ///     Attempts to remove and return the the value with the specified key from the <see cref="IConcurrentDictionary{TKey, TValue}" />.
        /// </summary>
        /// <param name="key">The key of the element to remove and return.</param>
        /// <param name="value">
        ///     When this method returns, <paramref name="value" /> contains the object removed from the
        ///     <see cref="IConcurrentDictionary{TKey,TValue}" /> or the default value of <typeparamref name="TValue" /> if the operation failed.
        /// </param>
        /// <returns>true if an object was removed successfully; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key" /> is a null reference (Nothing in Visual Basic).</exception>
        bool TryRemove(TKey key, out TValue value);

        /// <summary>
        ///     Attempts to get the value associated with the specified key from the <see cref="IConcurrentDictionary{TKey,TValue}" />.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">
        ///     When this method returns, <paramref name="value" /> contains the object from the
        ///     <see cref="IConcurrentDictionary{TKey,TValue}" /> with the specified key or the default value of
        ///     <typeparamref name="TValue" />, if the operation failed.
        /// </param>
        /// <returns>true if the key was found in the <see cref="IConcurrentDictionary{TKey,TValue}" />; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key" /> is a null reference (Nothing in Visual Basic).</exception>
        bool TryGetValue(TKey key, out TValue value);

        /// <summary>
        ///     Determines whether the <see cref="IConcurrentDictionary{TKey, TValue}" /> contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="IConcurrentDictionary{TKey, TValue}" />.</param>
        /// <returns>true if the <see cref="IConcurrentDictionary{TKey, TValue}" /> contains an element with the specified key; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key" /> is a null reference (Nothing in Visual Basic).</exception>
        bool ContainsKey(TKey key);

        /// <summary>
        ///     Compares the existing value for the specified key with a specified value, and if they're equal,
        ///     updates the key with a third value.
        /// </summary>
        /// <param name="key">
        ///     The key whose value is compared and possibly replaced.
        /// </param>
        /// <param name="newValue">
        ///     The value that replaces the value of the element with <paramref name="key" />
        ///     if the comparison results in equality.
        /// </param>
        /// <returns>
        ///     true if the value of <paramref name="key" /> is different than <paramref name="newValue" />
        ///     and therefore replaced; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="key" /> is a null reference.</exception>
        bool TryUpdate(TKey key, TValue newValue);

        /// <summary>
        ///     Adds a key/value pair to the <see cref="IConcurrentDictionary{TKey,TValue}" />
        ///     if the key does not already exist.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="createValue">The function used to generate a value for the key</param>
        /// <param name="value">
        ///     The value for the key. This will be either the existing value for the key if the
        ///     key is already in the dictionary, or the new value for the key as returned by valueFactory
        ///     if the key was not in the dictionary.
        /// </param>
        /// <returns>True if <paramref name="value" /> is created via <paramref name="createValue" /> and added to the dictionary, else false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key" /> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException"><paramref name="createValue" /> is a null reference (Nothing in Visual Basic).</exception>
        bool GetOrAdd(TKey key, Func<TValue> createValue, out TValue value);

        /// <summary>
        ///     Adds a key/value pair to the <see cref="IConcurrentDictionary{TKey,TValue}" />
        ///     if the key does not already exist.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="createValue">The function used to generate a value for the key</param>
        /// <returns>
        ///     The value for the key. This will be either the existing value for the key if the
        ///     key is already in the dictionary, or the new value for the key as returned by valueFactory
        ///     if the key was not in the dictionary.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="key" /> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException"><paramref name="createValue" /> is a null reference (Nothing in Visual Basic).</exception>
        TValue GetOrAdd(TKey key, Func<TValue> createValue);

        /// <summary>
        ///     Adds a key/value pair to the <see cref="IConcurrentDictionary{TKey,TValue}" /> if the key does not already
        ///     exist, or updates a key/value pair in the <see cref="IConcurrentDictionary{TKey,TValue}" /> if the key
        ///     already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="value">The value to be added for an absent key</param>
        /// <returns>True if <paramref name="value" /> is added to the dictionary, else false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key" /> is a null reference (Nothing in Visual Basic).</exception>
        bool AddOrUpdate(TKey key, TValue value);
    }
}