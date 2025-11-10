using System.Threading.Tasks;
using Soenneker.Validators.ExpiringKey.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;

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
        const string key = "test-key";
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
        const string key = "test-key";

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
        const string key = "test-key";
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
        const string key = "test-key";

        // Act
        _validator.Add(key, 1000);

        // Assert
        _validator.Validate(key).Should().BeFalse();
    }

    [Fact]
    public void Remove_ShouldRemoveKey()
    {
        // Arrange
        const string key = "test-key";
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
        const string key = "test-key";
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
        const string key1 = "test-key-1";
        const string key2 = "test-key-2";
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
        const string key1 = "test-key-1";
        const string key2 = "test-key-2";
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
        const int expirationTime = 1000; // 1 second

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
        const string key = "test-key";
        const int expirationTime = 1000; // 1 second
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
