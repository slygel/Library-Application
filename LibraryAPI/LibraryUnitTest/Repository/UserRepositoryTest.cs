using LibraryAPI.DbContext;
using LibraryAPI.Entities;
using LibraryAPI.Enums;
using LibraryAPI.Repository;
using Microsoft.EntityFrameworkCore;

namespace LibraryUnitTest.Repository
{
    class UserRepositoryTest
    {
        private AppDbContext _context;
        private UserRepository _repository;
        private List<User> _users;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "LibraryDb_" + Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _repository = new UserRepository(_context);

            // Create test users
            _users = new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                Name = "Admin User",
                Email = "admin@example.com",
                PhoneNumber = "1234567890",
                Address = "Admin Address",
                Username = "admin",
                Password = "admin_password",
                Role = Role.Admin
            },
            new User
            {
                Id = Guid.NewGuid(),
                Name = "Regular User",
                Email = "user@example.com",
                PhoneNumber = "0987654321",
                Address = "User Address",
                Username = "user1",
                Password = "user_password",
                Role = Role.User
            }
        };

            _context.Users.AddRange(_users);
            _context.SaveChanges();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task CreateAsync_ShouldAddNewUser()
        {
            // Arrange
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Name = "New User",
                Email = "newuser@example.com",
                PhoneNumber = "5555555555",
                Address = "New Address",
                Username = "newuser",
                Password = "new_password",
                Role = Role.User
            };

            // Act
            await _repository.CreateAsync(newUser);
            await _repository.SaveChangesAsync();

            // Assert
            var savedUser = await _context.Users.FindAsync(newUser.Id);
            Assert.That(savedUser, Is.Not.Null);
            Assert.That(savedUser.Name, Is.EqualTo("New User"));
            Assert.That(savedUser.Email, Is.EqualTo("newuser@example.com"));
            Assert.That(savedUser.Username, Is.EqualTo("newuser"));
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var existingUser = _users[0];

            // Act
            var result = await _repository.GetByIdAsync(existingUser.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(existingUser.Id));
            Assert.That(result.Name, Is.EqualTo(existingUser.Name));
            Assert.That(result.Username, Is.EqualTo(existingUser.Username));
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Act
            var result = await _repository.GetByIdAsync(Guid.NewGuid());

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetByUsernameAsync_ShouldReturnUser_WhenUsernameExists()
        {
            // Arrange
            var existingUser = _users[1];

            // Act
            var result = await _repository.GetByUsernameAsync(existingUser.Username);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(existingUser.Id));
            Assert.That(result.Username, Is.EqualTo(existingUser.Username));
        }

        [Test]
        public async Task GetByUsernameAsync_ShouldReturnNull_WhenUsernameDoesNotExist()
        {
            // Act
            var result = await _repository.GetByUsernameAsync("nonexistentuser");

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetByEmailAsync_ShouldReturnUser_WhenEmailExists()
        {
            // Arrange
            var existingUser = _users[0];

            // Act
            var result = await _repository.GetByEmailAsync(existingUser.Email);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(existingUser.Id));
            Assert.That(result.Email, Is.EqualTo(existingUser.Email));
        }

        [Test]
        public async Task GetByEmailAsync_ShouldReturnNull_WhenEmailDoesNotExist()
        {
            // Act
            var result = await _repository.GetByEmailAsync("nonexistent@example.com");

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task SaveChangesAsync_ShouldPersistChanges()
        {
            // Arrange
            var user = _users[0];
            user.Name = "Updated Name";
            user.Email = "updated@example.com";

            // Act
            await _repository.SaveChangesAsync();

            // Assert
            var updatedUser = await _context.Users.FindAsync(user.Id);
            Assert.That(updatedUser.Name, Is.EqualTo("Updated Name"));
            Assert.That(updatedUser.Email, Is.EqualTo("updated@example.com"));
        }
    }
}
