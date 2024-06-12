using Microsoft.Extensions.Logging;
using Soenneker.Validators.ExpiringKey.Abstract;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Extensions.ValueTask;

namespace Soenneker.Validators.ExpiringKey;

/// <inheritdoc cref="IExpiringKeyValidator"/>
public class ExpiringKeyValidator : Validator.Validator, IExpiringKeyValidator
{
    private readonly ConcurrentDictionary<string, Timer> _keyDict;

    public ExpiringKeyValidator(ILogger<ExpiringKeyValidator> logger) : base(logger)
    {
        _keyDict = new ConcurrentDictionary<string, Timer>();
    }

    public bool Validate(string key)
    {
        return _keyDict.ContainsKey(key);
    }

    public bool ValidateAndAdd(string key, int expirationTimeMilliseconds)
    {
        return _keyDict.TryAdd(key, CreateTimer(key, expirationTimeMilliseconds));
    }

    public void Add(string key, int expirationTimeMilliseconds)
    {
        _keyDict.TryAdd(key, CreateTimer(key, expirationTimeMilliseconds));
    }

    public void Remove(string key)
    {
        if (_keyDict.TryRemove(key, out Timer? timer))
        {
            timer.Dispose();
        }
    }

    private void Expire(object? state)
    {
        var key = (string) state!;

        Remove(key);
    }

    private Timer CreateTimer(string key, int expirationTimeMilliseconds)
    {
        return new Timer(Expire, key, expirationTimeMilliseconds, Timeout.Infinite);
    }

    public void Dispose()
    {
        foreach (Timer timer in _keyDict.Values)
        {
            timer.Dispose();
        }

        _keyDict.Clear();
    }

    public async ValueTask DisposeAsync()
    {
        foreach (Timer timer in _keyDict.Values)
        {
            await timer.DisposeAsync().NoSync();
        }

        _keyDict.Clear();
    }
}