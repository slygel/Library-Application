using LibraryAPI.DbContext;
using LibraryAPI.DTOs.StatisticDto;
using LibraryAPI.Entities;
using LibraryAPI.Enums;
using LibraryAPI.Repository;
using Microsoft.EntityFrameworkCore;

namespace LibraryUnitTest.Repository
{
    class StatisticRepositoryTest
    {
        private AppDbContext _context;
        private StatisticsRepository _repository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "LibraryDb_" + Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _repository = new StatisticsRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task GetStatisticsAsync_ShouldReturnCorrectCounts_WhenDatabaseHasData()
        {
            // Arrange
            var category = new Category { Id = Guid.NewGuid(), Name = "Test Category" };
            _context.Categories.Add(category);

            var books = new List<Book>
            {
                new Book
                {
                    Id = Guid.NewGuid(),
                    Title = "Design Patterns",
                    Author = "Gang of Four",
                    CategoryId = category.Id,
                    PublishDate = new DateOnly(1994, 10, 15),
                    Description = "A book about design patterns"
                }

            };
            _context.Books.AddRange(books);

            var users = new List<User>
            {
                new User 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "TaiTue",
                    Email = "admin@example.com",
                    PhoneNumber = "",
                    Address = "",
                    Username = "admin",
                    Password = "admin",
                    Role = Role.Admin 
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Name = "TaiTue",
                    Email = "user@example.com",
                    PhoneNumber = "",
                    Address = "",
                    Username = "user",
                    Password = "user",
                    Role = Role.User
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Name = "TaiTue",
                    Email = "user123@example.com",
                    PhoneNumber = "",
                    Address = "",
                    Username = "user123",
                    Password = "user123",
                    Role = Role.User
                },
            };
            _context.Users.AddRange(users);

            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetStatisticsAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.TotalBooks, Is.EqualTo(1));
            Assert.That(result.TotalCategories, Is.EqualTo(1));
            Assert.That(result.TotalUsers, Is.EqualTo(2)); // Expect 2 because admin is subtracted
        }
    }
}
