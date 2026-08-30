[![](https://img.shields.io/nuget/v/soenneker.validators.expiringkey.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.validators.expiringkey/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.validators.expiringkey/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.validators.expiringkey/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.validators.expiringkey.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.validators.expiringkey/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.validators.expiringkey/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.validators.expiringkey/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Validators.ExpiringKey
An in-memory, thread-safe expiring-key gate for suppressing duplicate work within a time window.

## Installation

```bash
dotnet add package Soenneker.Validators.ExpiringKey
```

## Registration

```csharp
using Soenneker.Validators.ExpiringKey.Registrars;

services.AddExpiringKeyValidatorAsSingleton();
```

Singleton registration shares keys across all callers in the process. Scoped registration creates an independent key set per dependency-injection scope. The validator requires an `ILogger<ExpiringKeyValidator>` when constructed manually.

## Atomic check-and-add

```csharp
bool accepted = validator.ValidateAndAdd(
    key: $"webhook:{eventId}",
    expirationTimeMilliseconds: 30_000);

if (!accepted)
    return; // The key already exists in the current window.
```

`ValidateAndAdd` is the safest operation for duplicate suppression because the existence check and insertion are atomic. It returns `true` only to the caller that added the key. The key is removed by its timer after the supplied interval.

## Individual operations

```csharp
validator.Add("job:42", 5_000);

bool available = validator.Validate("job:42");
// false: the key exists

validator.Remove("job:42");

available = validator.Validate("job:42");
// true: the key is absent
```

`Validate` has intentionally inverted gate semantics: it returns `true` when the key does not exist and `false` while the key is present. It does not reserve an absent key, so a separate `Validate` followed by `Add` is racy under concurrency; use `ValidateAndAdd` when only one caller may proceed.

`Add` attempts insertion and returns no result. If the key already exists, its original expiration is retained; the call does not refresh or replace its timer. `Remove` is idempotent and synchronously disposes the removed timer.

## Expiration and lifetime

Expiration is an in-process timer duration in milliseconds. Values below `-1` throw `ArgumentOutOfRangeException`; `-1` represents an infinite timeout. Expiration callbacks run asynchronously, so a zero-duration key may be observable briefly before its callback removes it.

This is not a distributed lock, durable cache, session store, or cross-process idempotency mechanism. Process restarts lose all keys, and separate validator instances do not share state.

The validator owns one timer per live key. Dispose it to release all timers; dependency injection handles disposal for registered instances. Calls after disposal follow the underlying dictionary's disposed-object behavior.

Keys use the underlying concurrent dictionary's default string comparison. Avoid placing secrets or unbounded attacker-controlled values into a long-lived singleton without applying size and cardinality limits at the caller.
