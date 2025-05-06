using LibraryAPI.DbContext;
using LibraryAPI.Entities;
using LibraryAPI.Repository;
using Microsoft.EntityFrameworkCore;

namespace LibraryUnitTest.Repository
{
    class BookRepositoryTest
    {
        private AppDbContext _context;
        private BookRepository _repository;
        private List<Book> _books;
        private Category _category;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "LibraryDb_" + Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _repository = new BookRepository(_context);

            // Create a test category
            _category = new Category { Id = Guid.NewGuid(), Name = "Test Category" };
            _context.Categories.Add(_category);
            _context.SaveChanges();

            // Create test books
            _books = new List<Book>
        {
            new Book
            {
                Id = Guid.NewGuid(),
                Title = "Clean Code",
                Author = "Robert C. Martin",
                CategoryId = _category.Id,
                PublishDate = new DateOnly(1990, 10, 15),
                Description = "A book about clean code"
            },
            new Book
            {
                Id = Guid.NewGuid(),
                Title = "Design Patterns",
                Author = "Gang of Four",
                CategoryId = _category.Id,
                PublishDate = new DateOnly(1994, 10, 15),
                Description = "A book about design patterns"
            }
        };

            _context.Books.AddRange(_books);
            _context.SaveChanges();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public void GetAll_ShouldReturnAllBooks_WhenNoFiltersApplied()
        {
            // Act
            var result = _repository.GetAll().ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.Select(b => b.Title), Does.Contain("Clean Code"));
            Assert.That(result.Select(b => b.Title), Does.Contain("Design Patterns"));
        }

        [Test]
        public void GetAll_ShouldReturnFilteredBooks_WhenCategoryIdProvided()
        {
            // Arrange
            var newCategory = new Category { Id = Guid.NewGuid(), Name = "New Category" };
            _context.Categories.Add(newCategory);

            var newBook = new Book
            {
                Id = Guid.NewGuid(),
                Title = "New Book",
                Author = "New Author",
                CategoryId = newCategory.Id
            };
            _context.Books.Add(newBook);
            _context.SaveChanges();

            // Act
            var result = _repository.GetAll(categoryId: _category.Id).ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.All(b => b.CategoryId == _category.Id), Is.True);
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnBook_WhenBookExists()
        {
            // Arrange
            var existingBook = _books[0];

            // Act
            var result = await _repository.GetByIdAsync(existingBook.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(existingBook.Id));
            Assert.That(result.Title, Is.EqualTo(existingBook.Title));
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnNull_WhenBookDoesNotExist()
        {
            // Act
            var result = await _repository.GetByIdAsync(Guid.NewGuid());

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task CreateAsync_ShouldAddNewBook()
        {
            // Arrange
            var newBook = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Test Book",
                Author = "Test Author",
                CategoryId = _category.Id
            };

            // Act
            var result = await _repository.CreateAsync(newBook);
            await _repository.SaveChangesAsync();

            // Assert
            var savedBook = await _context.Books.FindAsync(newBook.Id);
            Assert.That(savedBook, Is.Not.Null);
            Assert.That(savedBook.Title, Is.EqualTo("Test Book"));
            Assert.That(savedBook.Author, Is.EqualTo("Test Author"));
        }

        [Test]
        public async Task Update_ShouldUpdateExistingBook()
        {
            // Arrange
            var bookToUpdate = _books[0];
            bookToUpdate.Title = "Updated Title";
            bookToUpdate.Author = "Updated Author";

            // Act
            var result = _repository.Update(bookToUpdate);
            await _repository.SaveChangesAsync();

            // Assert
            var updatedBook = await _context.Books.FindAsync(bookToUpdate.Id);
            Assert.That(updatedBook.Title, Is.EqualTo("Updated Title"));
            Assert.That(updatedBook.Author, Is.EqualTo("Updated Author"));
        }

        [Test]
        public async Task Delete_ShouldRemoveBook()
        {
            // Arrange
            var bookToDelete = _books[0];

            // Act
            _repository.Delete(bookToDelete);
            await _repository.SaveChangesAsync();

            // Assert
            var deletedBook = await _context.Books.FindAsync(bookToDelete.Id);
            Assert.That(deletedBook, Is.Null);
        }

        [Test]
        public async Task GetBooksByCategoryAsync_ShouldReturnBooksInCategory()
        {
            // Act
            var result = await _repository.GetBooksByCategoryAsync(_category.Id);

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.All(b => b.CategoryId == _category.Id), Is.True);
        }

        [Test]
        public async Task GetBooksByCategoryAsync_ShouldReturnEmptyList_WhenNoBooksInCategory()
        {
            // Arrange
            var newCategory = new Category { Id = Guid.NewGuid(), Name = "Empty Category" };
            _context.Categories.Add(newCategory);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetBooksByCategoryAsync(newCategory.Id);

            // Assert
            Assert.That(result, Is.Empty);
        }
    }
}
