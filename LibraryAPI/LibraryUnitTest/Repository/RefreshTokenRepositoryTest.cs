using LibraryAPI.DbContext;
using LibraryAPI.Entities;
using LibraryAPI.Enums;
using LibraryAPI.Repository;
using Microsoft.EntityFrameworkCore;

namespace LibraryUnitTest.Repository
{
    class RefreshTokenRepositoryTest
    {
        private AppDbContext _context;
        private RefreshTokenRepository _repository;
        private User _testUser;
        private List<RefreshToken> _refreshTokens;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "LibraryDb_" + Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _repository = new RefreshTokenRepository(_context);

            // Create test user
            _testUser = new User
            {
                Id = Guid.NewGuid(),
                Name = "Test User",
                Email = "test@example.com",
                PhoneNumber = "1234567890",
                Address = "Test Address",
                Username = "testuser",
                Password = "test_password",
                Role = Role.User
            };
            _context.Users.Add(_testUser);
            _context.SaveChanges();

            // Create test refresh tokens
            _refreshTokens = new List<RefreshToken>
        {
            new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = "valid-token-1",
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow,
                UserId = _testUser.Id
            },
            new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = "expired-token",
                ExpiryDate = DateTime.UtcNow.AddDays(-1),
                IsRevoked = false,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                UserId = _testUser.Id
            },
            new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = "revoked-token",
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = true,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow,
                UserId = _testUser.Id
            }
        };

            _context.RefreshTokens.AddRange(_refreshTokens);
            _context.SaveChanges();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task GetByTokenAsync_ShouldReturnToken_WhenTokenExists()
        {
            // Arrange
            var existingToken = _refreshTokens[0].Token;

            // Act
            var result = await _repository.GetByTokenAsync(existingToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Token, Is.EqualTo(existingToken));
            Assert.That(result.User, Is.Not.Null);
            Assert.That(result.User.Id, Is.EqualTo(_testUser.Id));
        }

        [Test]
        public async Task GetByTokenAsync_ShouldReturnNull_WhenTokenDoesNotExist()
        {
            // Act
            var result = await _repository.GetByTokenAsync("non-existent-token");

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetByUserIdAsync_ShouldReturnAllUserTokens()
        {
            // Act
            var result = await _repository.GetByUserIdAsync(_testUser.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(3));
            Assert.That(result.Select(t => t.Token), Does.Contain("valid-token-1"));
            Assert.That(result.Select(t => t.Token), Does.Contain("expired-token"));
            Assert.That(result.Select(t => t.Token), Does.Contain("revoked-token"));
        }

        [Test]
        public async Task GetByUserIdAsync_ShouldReturnEmptyList_WhenUserHasNoTokens()
        {
            // Act
            var result = await _repository.GetByUserIdAsync(Guid.NewGuid());

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task CreateAsync_ShouldAddNewRefreshToken()
        {
            // Arrange
            var newToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = "new-token",
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow,
                UserId = _testUser.Id
            };

            // Act
            await _repository.CreateAsync(newToken);
            await _repository.SaveChangesAsync();

            // Assert
            var savedToken = await _context.RefreshTokens.FindAsync(newToken.Id);
            Assert.That(savedToken, Is.Not.Null);
            Assert.That(savedToken.Token, Is.EqualTo("new-token"));
            Assert.That(savedToken.UserId, Is.EqualTo(_testUser.Id));
        }

        [Test]
        public async Task Update_ShouldUpdateExistingRefreshToken()
        {
            // Arrange
            var tokenToUpdate = _refreshTokens[0];
            tokenToUpdate.IsUsed = true;

            // Act
            _repository.Update(tokenToUpdate);
            await _repository.SaveChangesAsync();

            // Assert
            var updatedToken = await _context.RefreshTokens.FindAsync(tokenToUpdate.Id);
            Assert.That(updatedToken.IsUsed, Is.True);
        }

        [Test]
        public async Task RevokeAllUserTokensAsync_ShouldRevokeAllActiveTokens()
        {
            // Act
            await _repository.RevokeAllUserTokensAsync(_testUser.Id);
            await _repository.SaveChangesAsync();

            // Assert
            var userTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == _testUser.Id)
                .ToListAsync();

            Assert.That(userTokens.Count, Is.EqualTo(3));
            Assert.That(userTokens.All(t => t.IsRevoked), Is.True);
        }

        [Test]
        public async Task SaveChangesAsync_ShouldPersistChanges()
        {
            // Arrange
            var token = _refreshTokens[1];
            token.IsUsed = true;
            token.IsRevoked = true;

            // Act
            await _repository.SaveChangesAsync();

            // Assert
            var updatedToken = await _context.RefreshTokens.FindAsync(token.Id);
            Assert.That(updatedToken.IsUsed, Is.True);
            Assert.That(updatedToken.IsRevoked, Is.True);
        }
    }
}
