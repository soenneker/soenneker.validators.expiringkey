using Soenneker.Validators.Validator.Abstract;
using System;

namespace Soenneker.Validators.ExpiringKey.Abstract;

/// <summary>
/// A validation module that checks for keys, stores them, expires them after an amount of time
/// </summary>
public interface IExpiringKeyValidator : IValidator, IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Validates if the provided key exists in the dictionary.
    /// </summary>
    /// <param name="key">The key to validate.</param>
    /// <returns>True if the key does not exist in the dictionary; otherwise, false.</returns>
    /// <exception cref="ObjectDisposedException">The validator has been disposed.</exception>
    bool Validate(string key);

    /// <summary>
    /// Validates if the provided key exists in the dictionary and adds it with an expiration timer if it does not.
    /// </summary>
    /// <param name="key">The key to validate and add.</param>
    /// <param name="expirationTimeMilliseconds">The expiration time in milliseconds for the key.</param>
    /// <returns>True if the key was successfully added; otherwise, false.</returns>
    /// <exception cref="ObjectDisposedException">The validator has been disposed.</exception>
    bool ValidateAndAdd(string key, int expirationTimeMilliseconds);

    /// <summary>
    /// Attempts to add the key with an expiration timer. An existing key and its timer are left unchanged.
    /// </summary>
    /// <param name="key">The key to add.</param>
    /// <param name="expirationTimeMilliseconds">The expiration time in milliseconds for the key.</param>
    /// <exception cref="ObjectDisposedException">The validator has been disposed.</exception>
    void Add(string key, int expirationTimeMilliseconds);

    /// <summary>
    /// Removes the provided key, when present, and synchronously disposes its associated timer.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    /// <exception cref="ObjectDisposedException">The validator has been disposed.</exception>
    void Remove(string key);
}
