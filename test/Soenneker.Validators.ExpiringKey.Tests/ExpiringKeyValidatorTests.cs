using System;
using System.Threading.Tasks;
using Soenneker.Validators.ExpiringKey;
using Soenneker.Validators.ExpiringKey.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;
using Microsoft.Extensions.Logging;

using AwesomeAssertions;

namespace Soenneker.Validators.ExpiringKey.Tests;

[Collection("Collection")]
public class ExpiringKeyValidatorTests : FixturedUnitTest
{
    private readonly IExpiringKeyValidator _validator;

    public ExpiringKeyValidatorTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _validator = Resolve<IExpiringKeyValidator>();
    }

    [Fact]
    public void Validate_ShouldReturnTrue_IfKeyDoesNotExist()
    {
        // Arrange
        string key = $"test-key-{Guid.NewGuid()}";

        // Act
        bool result = _validator.Validate(key);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldReturnFalse_IfKeyExists()
    {
        // Arrange
        string key = $"test-key-{Guid.NewGuid()}";
        _validator.Add(key, 1000);

        try
        {
            // Act
            bool result = _validator.Validate(key);

            // Assert
            result.Should().BeFalse();
        }
        finally
        {
            _validator.Remove(key);
        }
    }

    [Fact]
    public void ValidateAndAdd_ShouldAddKey_IfKeyDoesNotExist()
    {
        // Arrange
        string key = $"test-key-{Guid.NewGuid()}";

        try
        {
            // Act
            bool result = _validator.ValidateAndAdd(key, 1000);

            // Assert
            result.Should().BeTrue();
            _validator.Validate(key).Should().BeFalse();
        }
        finally
        {
            _validator.Remove(key);
        }
    }

    [Fact]
    public void ValidateAndAdd_ShouldNotAddKey_IfKeyExists()
    {
        // Arrange
        string key = $"test-key-{Guid.NewGuid()}";
        _validator.Add(key, 1000);

        try
        {
            // Act
            bool result = _validator.ValidateAndAdd(key, 1000);

            // Assert
            result.Should().BeFalse();
        }
        finally
        {
            _validator.Remove(key);
        }
    }

    [Fact]
    public void Add_ShouldAddKey()
    {
        // Arrange
        string key = $"test-key-{Guid.NewGuid()}";

        try
        {
            // Act
            _validator.Add(key, 1000);

            // Assert
            _validator.Validate(key).Should().BeFalse();
        }
        finally
        {
            _validator.Remove(key);
        }
    }

    [Fact]
    public void Remove_ShouldRemoveKey()
    {
        // Arrange
        string key = $"test-key-{Guid.NewGuid()}";
        _validator.Add(key, 1000);

        // Act
        _validator.Remove(key);

        // Assert
        _validator.Validate(key).Should().BeTrue();
    }

    [Fact]
    public async Task Expire_ShouldRemoveKeyAfterExpiration()
    {
        // Arrange
        string key = $"test-key-{Guid.NewGuid()}";
        _validator.Add(key, 500); // 0.5 seconds

        try
        {
            // Act
            await Task.Delay(1000); // Wait for the timer to expire

            // Assert
            _validator.Validate(key).Should().BeTrue();
        }
        finally
        {
            _validator.Remove(key);
        }
    }

    [Fact]
    public void Dispose_ShouldDisposeAllTimers()
    {
        // Arrange - create a separate validator instance for this test
        var logger = Resolve<ILogger<ExpiringKeyValidator>>();
        var validator = new ExpiringKeyValidator(logger);
        string key1 = $"test-key-1-{Guid.NewGuid()}";
        string key2 = $"test-key-2-{Guid.NewGuid()}";
        validator.Add(key1, 1000);
        validator.Add(key2, 1000);

        // Act
        validator.Dispose();

        // Assert
        validator.Validate(key1).Should().BeTrue();
        validator.Validate(key2).Should().BeTrue();
    }

    [Fact]
    public async Task DisposeAsync_ShouldDisposeAllTimersAsync()
    {
        // Arrange - create a separate validator instance for this test
        var logger = Resolve<ILogger<ExpiringKeyValidator>>();
        var validator = new ExpiringKeyValidator(logger);
        string key1 = $"test-key-1-{Guid.NewGuid()}";
        string key2 = $"test-key-2-{Guid.NewGuid()}";
        validator.Add(key1, 1000);
        validator.Add(key2, 1000);

        // Act
        await validator.DisposeAsync();

        // Assert
        validator.Validate(key1).Should().BeTrue();
        validator.Validate(key2).Should().BeTrue();
    }

    [Fact]
    public async Task Parallel_AddAndValidate_ShouldWorkCorrectly()
    {
        // Arrange
        string testId = Guid.NewGuid().ToString();
        var keys = new[] 
        { 
            $"key1-{testId}", 
            $"key2-{testId}", 
            $"key3-{testId}", 
            $"key4-{testId}", 
            $"key5-{testId}" 
        };
        const int expirationTime = 1000; // 1 second

        try
        {
            // Act
            Parallel.ForEach(keys, key =>
            {
                _validator.ValidateAndAdd(key, expirationTime);
            });

            // Assert
            Parallel.ForEach(keys, key =>
            {
                _validator.Validate(key).Should().BeFalse(); // Should be false because the keys should exist
            });

            // Wait for all keys to expire
            await Task.Delay(2000);

            // Assert keys are expired
            Parallel.ForEach(keys, key =>
            {
                _validator.Validate(key).Should().BeTrue(); // Should be true because the keys should be expired
            });
        }
        finally
        {
            // Cleanup
            Parallel.ForEach(keys, key => _validator.Remove(key));
        }
    }

    [Fact]
    public async Task Parallel_ValidateAndAdd_SameKey_ShouldWorkCorrectly()
    {
        // Arrange
        string key = $"test-key-{Guid.NewGuid()}";
        const int expirationTime = 1000; // 1 second
        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 10 }; // Adjust the degree of parallelism as needed

        try
        {
            // Act
            Parallel.For(0, 100, parallelOptions, i =>
            {
                _validator.ValidateAndAdd(key, expirationTime);
            });

            // Assert
            _validator.Validate(key).Should().BeFalse(); // Key should exist

            // Wait for the key to expire
            await Task.Delay(2000);

            // Assert the key has expired
            _validator.Validate(key).Should().BeTrue(); // Key should be expired
        }
        finally
        {
            _validator.Remove(key);
        }
    }
}
