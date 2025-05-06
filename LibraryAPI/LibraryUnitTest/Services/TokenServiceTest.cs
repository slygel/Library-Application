using LibraryAPI.Entities;
using LibraryAPI.Enums;
using LibraryAPI.IRepository;
using LibraryAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;

namespace LibraryUnitTest.Services
{
    class TokenServiceTest
    {
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<IRefreshTokenRepository> _mockRefreshTokenRepository;
        private TokenService _tokenService;
        private User _testUser;
        private RefreshToken _testRefreshToken;
        private RefreshToken _expiredRefreshToken;
        private RefreshToken _revokedRefreshToken;
        private RefreshToken _usedRefreshToken;

        [SetUp]
        public void Setup()
        {
            // Create test configuration
            var inMemorySettings = new Dictionary<string, string> {
                {"JwtConfig:Issuer", "http://testissuer.com"},
                {"JwtConfig:Audience", "http://testaudience.com"},
                {"JwtConfig:Key", "testkey_that_is_long_enough_for_jwt_hmacsha256_signature"},
                {"JwtConfig:TokenValidityMins", "60"},
                {"JwtConfig:RefreshTokenValidityDays", "7"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _mockConfiguration = new Mock<IConfiguration>();
            // Set up indexer to return values from our in-memory collection
            _mockConfiguration.Setup(x => x[It.Is<string>(s => inMemorySettings.ContainsKey(s))])
                .Returns<string>(x => inMemorySettings[x]);

            // For GetValue<T> calls, we need to properly setup a method to handle any type
            _mockConfiguration.Setup(c => c.GetSection("JwtConfig:TokenValidityMins").Value).Returns("60");
            _mockConfiguration.Setup(c => c.GetSection("JwtConfig:RefreshTokenValidityDays").Value).Returns("7");

            _mockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();

            // Instead of using the mock directly, use the real configuration
            _tokenService = new TokenService(configuration, _mockRefreshTokenRepository.Object);

            // Setup test user
            _testUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Password = "hashed_password",
                Email = "test@example.com",
                Name = "Test User",
                PhoneNumber = "1234567890",
                Address = "Test Address",
                Role = Role.User,
                RefreshTokens = new List<RefreshToken>()
            };

            // Setup test refresh tokens
            _testRefreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = "valid-refresh-token",
                UserId = _testUser.Id,
                User = _testUser,
                CreatedAt = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                IsUsed = false
            };

            _expiredRefreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = "expired-refresh-token",
                UserId = _testUser.Id,
                User = _testUser,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                ExpiryDate = DateTime.UtcNow.AddDays(-3),
                IsRevoked = false,
                IsUsed = false
            };

            _revokedRefreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = "revoked-refresh-token",
                UserId = _testUser.Id,
                User = _testUser,
                CreatedAt = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = true,
                IsUsed = false
            };

            _usedRefreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = "used-refresh-token",
                UserId = _testUser.Id,
                User = _testUser,
                CreatedAt = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                IsUsed = true
            };
        }

        [Test]
        public async Task CreateRefreshTokenAsync_ShouldRevokeExistingTokensAndCreateNewOne()
        {
            // Arrange
            _mockRefreshTokenRepository.Setup(repo => repo.RevokeAllUserTokensAsync(_testUser.Id))
                .Returns(Task.CompletedTask);
            _mockRefreshTokenRepository.Setup(repo => repo.CreateAsync(It.IsAny<RefreshToken>()))
                .Returns(Task.CompletedTask);
            _mockRefreshTokenRepository.Setup(repo => repo.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _tokenService.CreateRefreshTokenAsync(_testUser);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Token, Is.Not.Null);
            Assert.That(result.Token, Is.Not.Empty);
            Assert.That(result.UserId, Is.EqualTo(_testUser.Id));
            Assert.That(result.IsRevoked, Is.False);
            Assert.That(result.IsUsed, Is.False);
            Assert.That(result.ExpiryDate, Is.GreaterThan(DateTime.UtcNow.AddDays(6.9)));
            Assert.That(result.ExpiryDate, Is.LessThan(DateTime.UtcNow.AddDays(7.1)));

            _mockRefreshTokenRepository.Verify(repo => repo.RevokeAllUserTokensAsync(_testUser.Id), Times.Once);
            _mockRefreshTokenRepository.Verify(repo => repo.CreateAsync(It.IsAny<RefreshToken>()), Times.Once);
            _mockRefreshTokenRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task RefreshTokenAsync_ShouldReturnSuccess_WhenTokenIsValid()
        {
            // Arrange
            _mockRefreshTokenRepository.Setup(repo => repo.GetByTokenAsync(_testRefreshToken.Token))
                .ReturnsAsync(_testRefreshToken);
            _mockRefreshTokenRepository.Setup(repo => repo.Update(It.IsAny<RefreshToken>()));
            _mockRefreshTokenRepository.Setup(repo => repo.RevokeAllUserTokensAsync(_testUser.Id))
                .Returns(Task.CompletedTask);
            _mockRefreshTokenRepository.Setup(repo => repo.CreateAsync(It.IsAny<RefreshToken>()))
                .Returns(Task.CompletedTask);
            _mockRefreshTokenRepository.Setup(repo => repo.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _tokenService.RefreshTokenAsync(_testRefreshToken.Token);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.AccessToken, Is.Not.Null);
            Assert.That(result.Value.RefreshToken, Is.Not.Null);

            _mockRefreshTokenRepository.Verify(repo => repo.Update(_testRefreshToken), Times.Once);
            Assert.That(_testRefreshToken.IsUsed, Is.True);
            _mockRefreshTokenRepository.Verify(repo => repo.SaveChangesAsync(), Times.AtLeast(1));
        }

        [Test]
        public async Task RefreshTokenAsync_ShouldReturnFailure_WhenTokenNotFound()
        {
            // Arrange
            string nonExistentToken = "non-existent-token";
            _mockRefreshTokenRepository.Setup(repo => repo.GetByTokenAsync(nonExistentToken))
                .ReturnsAsync((RefreshToken)null);

            // Act
            var result = await _tokenService.RefreshTokenAsync(nonExistentToken);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Invalid refresh token"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task RefreshTokenAsync_ShouldReturnFailure_WhenTokenIsExpired()
        {
            // Arrange
            _mockRefreshTokenRepository.Setup(repo => repo.GetByTokenAsync(_expiredRefreshToken.Token))
                .ReturnsAsync(_expiredRefreshToken);

            // Act
            var result = await _tokenService.RefreshTokenAsync(_expiredRefreshToken.Token);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Refresh token expired"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task RefreshTokenAsync_ShouldReturnFailure_WhenTokenIsRevoked()
        {
            // Arrange
            _mockRefreshTokenRepository.Setup(repo => repo.GetByTokenAsync(_revokedRefreshToken.Token))
                .ReturnsAsync(_revokedRefreshToken);

            // Act
            var result = await _tokenService.RefreshTokenAsync(_revokedRefreshToken.Token);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Refresh token revoked"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task RefreshTokenAsync_ShouldReturnFailure_WhenTokenIsAlreadyUsed()
        {
            // Arrange
            _mockRefreshTokenRepository.Setup(repo => repo.GetByTokenAsync(_usedRefreshToken.Token))
                .ReturnsAsync(_usedRefreshToken);

            // Act
            var result = await _tokenService.RefreshTokenAsync(_usedRefreshToken.Token);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Refresh token already used"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task GetRefreshTokenAsync_ShouldReturnToken_WhenTokenExists()
        {
            // Arrange
            _mockRefreshTokenRepository.Setup(repo => repo.GetByTokenAsync(_testRefreshToken.Token))
                .ReturnsAsync(_testRefreshToken);

            // Act
            var result = await _tokenService.GetRefreshTokenAsync(_testRefreshToken.Token);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Token, Is.EqualTo(_testRefreshToken.Token));
        }

        [Test]
        public async Task GetRefreshTokenAsync_ShouldReturnNull_WhenTokenDoesNotExist()
        {
            // Arrange
            string nonExistentToken = "non-existent-token";
            _mockRefreshTokenRepository.Setup(repo => repo.GetByTokenAsync(nonExistentToken))
                .ReturnsAsync((RefreshToken)null);

            // Act
            var result = await _tokenService.GetRefreshTokenAsync(nonExistentToken);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task UpdateRefreshTokenAsync_ShouldUpdateToken()
        {
            // Arrange
            _mockRefreshTokenRepository.Setup(repo => repo.Update(_testRefreshToken));
            _mockRefreshTokenRepository.Setup(repo => repo.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _tokenService.UpdateRefreshTokenAsync(_testRefreshToken);

            // Assert
            _mockRefreshTokenRepository.Verify(repo => repo.Update(_testRefreshToken), Times.Once);
            _mockRefreshTokenRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }
    }
}
