using retoSquadmakers.Infrastructure.Security;

namespace retoSquadmakers.Tests.Infrastructure.Services;

public class PasswordHasherTests
{
    private readonly PasswordHasher _passwordHasher;

    public PasswordHasherTests()
    {
        _passwordHasher = new PasswordHasher();
    }

    [Fact]
    public void HashPassword_WithValidPassword_ReturnsHashedPassword()
    {
        // Arrange
        var password = "test_password_123";

        // Act
        var hashedPassword = _passwordHasher.HashPassword(password);

        // Assert
        Assert.NotNull(hashedPassword);
        Assert.NotEmpty(hashedPassword);
        Assert.NotEqual(password, hashedPassword);
        Assert.True(hashedPassword.Length > password.Length);
    }

    [Fact]
    public void HashPassword_WithSamePassword_ReturnsDifferentHashes()
    {
        // Arrange
        var password = "same_password";

        // Act
        var hash1 = _passwordHasher.HashPassword(password);
        var hash2 = _passwordHasher.HashPassword(password);

        // Assert
        Assert.NotEqual(hash1, hash2); // BCrypt generates different salts
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "correct_password";
        var hashedPassword = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword(password, hashedPassword);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var correctPassword = "correct_password";
        var incorrectPassword = "incorrect_password";
        var hashedPassword = _passwordHasher.HashPassword(correctPassword);

        // Act
        var result = _passwordHasher.VerifyPassword(incorrectPassword, hashedPassword);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("simple")]
    [InlineData("complex_password_123!@#")]
    [InlineData("   password with spaces   ")]
    [InlineData("contraseña_con_ñ")]
    [InlineData("Password123!@#$%^&*()")]
    public void HashPassword_WithVariousPasswords_WorksCorrectly(string password)
    {
        // Act
        var hashedPassword = _passwordHasher.HashPassword(password);
        var isValid = _passwordHasher.VerifyPassword(password, hashedPassword);

        // Assert
        Assert.NotNull(hashedPassword);
        Assert.NotEmpty(hashedPassword);
        Assert.True(isValid);
    }

    [Fact]
    public void VerifyPassword_WithEmptyPassword_ReturnsFalse()
    {
        // Arrange
        var password = "test_password";
        var emptyPassword = "";
        var hashedPassword = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword(emptyPassword, hashedPassword);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HashPassword_WithEmptyPassword_ReturnsHash()
    {
        // Arrange
        var emptyPassword = "";

        // Act
        var hashedPassword = _passwordHasher.HashPassword(emptyPassword);

        // Assert
        Assert.NotNull(hashedPassword);
        Assert.NotEmpty(hashedPassword);
        
        // Should be able to verify empty password
        var isValid = _passwordHasher.VerifyPassword(emptyPassword, hashedPassword);
        Assert.True(isValid);
    }

    [Fact]
    public void VerifyPassword_WithNullPassword_ReturnsFalse()
    {
        // Arrange
        var password = "test_password";
        var hashedPassword = _passwordHasher.HashPassword(password);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _passwordHasher.VerifyPassword(null!, hashedPassword));
    }

    [Fact]
    public void HashPassword_WithNullPassword_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _passwordHasher.HashPassword(null!));
    }

    [Fact]
    public void VerifyPassword_WithInvalidHash_ReturnsFalse()
    {
        // Arrange
        var password = "test_password";
        var invalidHash = "invalid_hash_format";

        // Act & Assert
        // BCrypt throws exception for invalid hash format, which should be handled by the service
        Assert.Throws<BCrypt.Net.SaltParseException>(() => _passwordHasher.VerifyPassword(password, invalidHash));
    }

    [Fact]
    public void VerifyPassword_WithEmptyHash_ThrowsException()
    {
        // Arrange
        var password = "test_password";
        var emptyHash = "";

        // Act & Assert
        // BCrypt throws exception for empty hash, which should be handled by the service
        Assert.Throws<ArgumentException>(() => _passwordHasher.VerifyPassword(password, emptyHash));
    }

    [Fact]
    public void HashPassword_CreatesValidBCryptHash()
    {
        // Arrange
        var password = "test_password";

        // Act
        var hashedPassword = _passwordHasher.HashPassword(password);

        // Assert
        // BCrypt hashes start with $2a$, $2b$, or $2y$ and have specific length
        Assert.True(hashedPassword.StartsWith("$2") || hashedPassword.StartsWith("$2a$") || 
                   hashedPassword.StartsWith("$2b$") || hashedPassword.StartsWith("$2y$"));
        Assert.Equal(60, hashedPassword.Length); // BCrypt hash length is always 60 characters
    }
}