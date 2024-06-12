[![](https://img.shields.io/nuget/v/soenneker.validators.expiringkey.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.validators.expiringkey/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.validators.expiringkey/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.validators.expiringkey/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.validators.expiringkey.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.validators.expiringkey/)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Validators.ExpiringKey
### A validation module that checks for keys, stores them, expires them after an amount of time

Ideal for caching, session management, and more.

## 🚀 Features

- **Validate Key**: Check if a key exists.
- **Add Key**: Add a key with an expiration time.
- **Validate and Add**: Validate if a key exists and add it if not.
- **Remove Key**: Remove a key.

## Installation

```
dotnet add package Soenneker.Validators.ExpiringKey
```

## 💻 Usage

`IExpiringKeyValidator` can be registered within DI, and injected:

```csharp
public static async Task Main(string[] args)
{
    ...
    builder.Services.AddExpiringKeyValidatorAsSingleton();
}
```

or it can be initialized manually: `new ExpiringKeyValidator()`.

### Validate Key

Check if a key is present.

```csharp
bool Validate(string key)
```

### Add Key

Add a key with an expiration time.

```csharp
void Add(string key, int expirationTimeMilliseconds)
```

### Validate and Add Key

Validate a key and add it if it doesn't exist.

```csharp
bool ValidateAndAdd(string key, int expirationTimeMilliseconds) // true if doesn't exist, false if it does
```

### Remove Key

Remove a key.

```csharp
void Remove(string key)
```

## Example

```csharp
var validator = new ExpiringKeyValidator();
validator.Add("key1", 5000); // 5 seconds

var invalid = validator.Validate("key1"); // false, key exists

await Task.Delay(7000); // wait 7 seconds

var validAfterTime = validator.Validate("key1"); // true, key does not exist
```