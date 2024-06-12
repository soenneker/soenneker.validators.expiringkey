using System.Threading.Tasks;
using Soenneker.Validators.ExpiringKey.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;
using Xunit.Abstractions;
using FluentAssertions;

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
        const string key = "test-key";

        // Act
        bool result = _validator.Validate(key);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldReturnFalse_IfKeyExists()
    {
        // Arrange
        var key = "test-key";
        _validator.Add(key, 1000);

        // Act
        bool result = _validator.Validate(key);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateAndAdd_ShouldAddKey_IfKeyDoesNotExist()
    {
        // Arrange
        var key = "test-key";

        // Act
        bool result = _validator.ValidateAndAdd(key, 1000);

        // Assert
        result.Should().BeTrue();
        _validator.Validate(key).Should().BeFalse();
    }

    [Fact]
    public void ValidateAndAdd_ShouldNotAddKey_IfKeyExists()
    {
        // Arrange
        var key = "test-key";
        _validator.Add(key, 1000);

        // Act
        bool result = _validator.ValidateAndAdd(key, 1000);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Add_ShouldAddKey()
    {
        // Arrange
        var key = "test-key";

        // Act
        _validator.Add(key, 1000);

        // Assert
        _validator.Validate(key).Should().BeFalse();
    }

    [Fact]
    public void Remove_ShouldRemoveKey()
    {
        // Arrange
        var key = "test-key";
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
        var key = "test-key";
        _validator.Add(key, 500); // 0.5 seconds

        // Act
        await Task.Delay(1000); // Wait for the timer to expire

        // Assert
        _validator.Validate(key).Should().BeTrue();
    }

    [Fact]
    public void Dispose_ShouldDisposeAllTimers()
    {
        // Arrange
        var key1 = "test-key-1";
        var key2 = "test-key-2";
        _validator.Add(key1, 1000);
        _validator.Add(key2, 1000);

        // Act
        _validator.Dispose();

        // Assert
        _validator.Validate(key1).Should().BeTrue();
        _validator.Validate(key2).Should().BeTrue();
    }

    [Fact]
    public async Task DisposeAsync_ShouldDisposeAllTimersAsync()
    {
        // Arrange
        var key1 = "test-key-1";
        var key2 = "test-key-2";
        _validator.Add(key1, 1000);
        _validator.Add(key2, 1000);

        // Act
        await _validator.DisposeAsync();

        // Assert
        _validator.Validate(key1).Should().BeTrue();
        _validator.Validate(key2).Should().BeTrue();
    }

    [Fact]
    public async Task Parallel_AddAndValidate_ShouldWorkCorrectly()
    {
        // Arrange
        var keys = new[] { "key1", "key2", "key3", "key4", "key5" };
        var expirationTime = 1000; // 1 second

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

    [Fact]
    public async Task Parallel_ValidateAndAdd_SameKey_ShouldWorkCorrectly()
    {
        // Arrange
        var key = "test-key";
        var expirationTime = 1000; // 1 second
        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 10 }; // Adjust the degree of parallelism as needed

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
}
