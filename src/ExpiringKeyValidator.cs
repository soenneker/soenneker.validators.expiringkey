using Microsoft.Extensions.Logging;
using Soenneker.Validators.ExpiringKey.Abstract;
using System.Threading.Tasks;
using Soenneker.Dictionaries.ExpiringKey;

namespace Soenneker.Validators.ExpiringKey;

/// <inheritdoc cref="IExpiringKeyValidator"/>
public class ExpiringKeyValidator : Validator.Validator, IExpiringKeyValidator
{
    private readonly ExpiringKeyDictionary _keyDict;

    public ExpiringKeyValidator(ILogger<ExpiringKeyValidator> logger) : base(logger)
    {
        _keyDict = new ExpiringKeyDictionary();
    }

    public bool Validate(string key)
    {
        return !_keyDict.ContainsKey(key);
    }

    public bool ValidateAndAdd(string key, int expirationTimeMilliseconds)
    {
        return _keyDict.TryAdd(key, expirationTimeMilliseconds);
    }

    public void Add(string key, int expirationTimeMilliseconds)
    {
        _keyDict.TryAdd(key, expirationTimeMilliseconds);
    }

    public void Remove(string key)
    {
        _keyDict.TryRemove(key);
    }

    public void Dispose()
    {
        _keyDict.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return _keyDict.DisposeAsync();
    }
}