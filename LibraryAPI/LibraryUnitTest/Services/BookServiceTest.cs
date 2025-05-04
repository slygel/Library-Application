using LibraryAPI.DTOs.BookDto;
using LibraryAPI.Entities;
using LibraryAPI.IRepository;
using LibraryAPI.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using Microsoft.EntityFrameworkCore;
using LibraryAPI.Extensions;

namespace LibraryUnitTest.Services
{
    class BookServiceTest
    {
        private Mock<IBookRepository> _mockBookRepository;
        private Mock<ICategoryRepository> _mockCategoryRepository;
        private BookService _bookService;
        private List<Book> _books;
        private Category _category;

        [SetUp]
        public void Setup()
        {
            _mockBookRepository = new Mock<IBookRepository>();
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _bookService = new BookService(_mockBookRepository.Object, _mockCategoryRepository.Object);

            // Setup test category
            _category = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Test Category",
                Description = "Test Category Description"
            };

            // Setup test books
            _books = new List<Book>
            {
                new Book
                {
                    Id = Guid.NewGuid(),
                    Title = "Book 1",
                    Author = "Author 1",
                    CategoryId = _category.Id,
                    Category = _category,
                    PublishDate = new DateOnly(2020, 1, 1),
                    Isbn = "ISBN-1",
                    Quantity = 10,
                    AvailableQuantity = 8,
                    Description = "Description 1"
                },
                new Book
                {
                    Id = Guid.NewGuid(),
                    Title = "Book 2",
                    Author = "Author 2",
                    CategoryId = _category.Id,
                    Category = _category,
                    PublishDate = new DateOnly(2021, 2, 2),
                    Isbn = "ISBN-2",
                    Quantity = 5,
                    AvailableQuantity = 5,
                    Description = "Description 2"
                },
                new Book
                {
                    Id = Guid.NewGuid(),
                    Title = "Book 3",
                    Author = "Author 3",
                    CategoryId = _category.Id,
                    Category = _category,
                    PublishDate = new DateOnly(2022, 3, 3),
                    Isbn = "ISBN-3",
                    Quantity = 3,
                    AvailableQuantity = 0,
                    Description = "Description 3"
                }
            };
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnAllBooks_WhenNoFiltersProvided()
        {
            // Arrange
            var queryableBooks = _books.AsQueryable();
            _mockBookRepository.Setup(repo => repo.GetAll(null, null)).Returns(queryableBooks);

            // Mock the async enumeration behavior
            // This is necessary because PaginatedList.CreateAsync calls CountAsync and ToListAsync
            var mockRepo = _mockBookRepository.Object;
            var books = mockRepo.GetAll(null, null);
            var count = books.Count();
            var bookResponses = books.Select(b => b.ToBookResponse()).ToList();

            // Act
            var result = await _bookService.GetAllAsync(1, 10);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Items.Count(), Is.EqualTo(3));
            Assert.That(result.PageIndex, Is.EqualTo(1));
            Assert.That(result.TotalPages, Is.EqualTo(1));
            Assert.That(result.TotalItems, Is.EqualTo(3));
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnFilteredBooks_WhenTitleFilterProvided()
        {
            // Arrange
            var filteredBooks = _books.Where(b => b.Title.Contains("Book 1")).AsQueryable();
            _mockBookRepository.Setup(repo => repo.GetAll("Book 1", null)).Returns(filteredBooks);

            // Mock the async enumeration behavior
            var mockRepo = _mockBookRepository.Object;
            var books = mockRepo.GetAll("Book 1", null);
            var count = books.Count();
            var bookResponses = books.Select(b => b.ToBookResponse()).ToList();

            // Act
            var result = await _bookService.GetAllAsync(1, 10, "Book 1");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Items.Count(), Is.EqualTo(1));
            Assert.That(result.Items.First().Title, Is.EqualTo("Book 1"));
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnFilteredBooks_WhenCategoryFilterProvided()
        {
            // Arrange
            var categoryId = _category.Id;
            var filteredBooks = _books.Where(b => b.CategoryId == categoryId).AsQueryable();
            _mockBookRepository.Setup(repo => repo.GetAll(null, categoryId)).Returns(filteredBooks);

            // Act
            var result = await _bookService.GetAllAsync(1, 10, null, categoryId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Items.Count(), Is.EqualTo(3));
            Assert.That(result.Items.All(b => b.CategoryId == categoryId), Is.True);
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnBook_WhenBookExists()
        {
            // Arrange
            var bookId = _books[0].Id;
            _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
                .ReturnsAsync(_books[0]);

            // Act
            var result = await _bookService.GetByIdAsync(bookId);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.Id, Is.EqualTo(bookId));
            Assert.That(result.Value.Title, Is.EqualTo(_books[0].Title));
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnFailure_WhenBookDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            _mockBookRepository.Setup(repo => repo.GetByIdAsync(nonExistentId))
                .ReturnsAsync((Book)null);

            // Act
            var result = await _bookService.GetByIdAsync(nonExistentId);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Book not found"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        }

        [Test]
        public async Task CreateAsync_ShouldCreateBook_WhenRequestIsValid()
        {
            // Arrange
            var categoryId = _category.Id;
            var bookRequest = new BookRequest
            {
                Title = "New Book",
                Author = "New Author",
                CategoryId = categoryId,
                PublishDate = new DateOnly(2023, 1, 1),
                Isbn = "012345678901",
                Quantity = 5,
                AvailableQuantity = 5,
                Description = "New Description"
            };

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync(_category);
            _mockBookRepository.Setup(repo => repo.CreateAsync(It.IsAny<Book>()))
                .ReturnsAsync((Book b) => b);

            // Act
            var result = await _bookService.CreateAsync(bookRequest);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.Title, Is.EqualTo(bookRequest.Title));
            Assert.That(result.Value.Author, Is.EqualTo(bookRequest.Author));
            Assert.That(result.Value.CategoryId, Is.EqualTo(categoryId));

            _mockBookRepository.Verify(repo => repo.CreateAsync(It.IsAny<Book>()), Times.Once);
            _mockBookRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task CreateAsync_ShouldReturnFailure_WhenCategoryDoesNotExist()
        {
            // Arrange
            var nonExistentCategoryId = Guid.NewGuid();
            var bookRequest = new BookRequest
            {
                Title = "New Book",
                Author = "New Author",
                CategoryId = nonExistentCategoryId,
                PublishDate = new DateOnly(2023, 1, 1),
                Quantity = 5,
                AvailableQuantity = 5
            };

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(nonExistentCategoryId))
                .ReturnsAsync((Category)null);

            // Act
            var result = await _bookService.CreateAsync(bookRequest);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Category not found"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));

            _mockBookRepository.Verify(repo => repo.CreateAsync(It.IsAny<Book>()), Times.Never);
            _mockBookRepository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public async Task CreateAsync_ShouldReturnFailure_WhenAvailableQuantityExceedsQuantity()
        {
            // Arrange
            var categoryId = _category.Id;
            var bookRequest = new BookRequest
            {
                Title = "New Book",
                Author = "New Author",
                CategoryId = categoryId,
                PublishDate = new DateOnly(2023, 1, 1),
                Quantity = 5,
                AvailableQuantity = 10, // Invalid: available > quantity
                Description = "New Description"
            };

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync(_category);

            // Act
            var result = await _bookService.CreateAsync(bookRequest);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Available can't bigger quantity"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));

            _mockBookRepository.Verify(repo => repo.CreateAsync(It.IsAny<Book>()), Times.Never);
            _mockBookRepository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public async Task UpdateAsync_ShouldUpdateBook_WhenRequestIsValid()
        {
            // Arrange
            var bookId = _books[0].Id;
            var categoryId = _category.Id;
            var bookRequest = new BookRequest
            {
                Title = "Updated Book",
                Author = "Updated Author",
                CategoryId = categoryId,
                PublishDate = new DateOnly(2023, 5, 5),
                Isbn = "UPDATED-ISBN",
                Quantity = 15,
                AvailableQuantity = 12,
                Description = "Updated Description"
            };

            _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
                .ReturnsAsync(_books[0]);
            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync(_category);
            _mockBookRepository.Setup(repo => repo.Update(It.IsAny<Book>()))
                .Returns((Book b) => b);

            // Act
            var result = await _bookService.UpdateAsync(bookId, bookRequest);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.Title, Is.EqualTo(bookRequest.Title));
            Assert.That(result.Value.Author, Is.EqualTo(bookRequest.Author));
            Assert.That(result.Value.Description, Is.EqualTo(bookRequest.Description));

            _mockBookRepository.Verify(repo => repo.Update(It.IsAny<Book>()), Times.Once);
            _mockBookRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task UpdateAsync_ShouldReturnFailure_WhenBookDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var bookRequest = new BookRequest
            {
                Title = "Updated Book",
                Author = "Updated Author",
                CategoryId = _category.Id,
                PublishDate = new DateOnly(2023, 5, 5),
                Quantity = 15,
                AvailableQuantity = 12
            };

            _mockBookRepository.Setup(repo => repo.GetByIdAsync(nonExistentId))
                .ReturnsAsync((Book)null);

            // Act
            var result = await _bookService.UpdateAsync(nonExistentId, bookRequest);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Book not found"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));

            _mockBookRepository.Verify(repo => repo.Update(It.IsAny<Book>()), Times.Never);
            _mockBookRepository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public async Task UpdateAsync_ShouldReturnFailure_WhenCategoryDoesNotExist()
        {
            // Arrange
            var bookId = _books[0].Id;
            var nonExistentCategoryId = Guid.NewGuid();
            var bookRequest = new BookRequest
            {
                Title = "Updated Book",
                Author = "Updated Author",
                CategoryId = nonExistentCategoryId,
                PublishDate = new DateOnly(2023, 5, 5),
                Quantity = 15,
                AvailableQuantity = 12
            };

            _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
                .ReturnsAsync(_books[0]);
            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(nonExistentCategoryId))
                .ReturnsAsync((Category)null);

            // Act
            var result = await _bookService.UpdateAsync(bookId, bookRequest);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Category not found"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));

            _mockBookRepository.Verify(repo => repo.Update(It.IsAny<Book>()), Times.Never);
            _mockBookRepository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public async Task UpdateAsync_ShouldReturnFailure_WhenAvailableQuantityExceedsQuantity()
        {
            // Arrange
            var bookId = _books[0].Id;
            var categoryId = _category.Id;
            var bookRequest = new BookRequest
            {
                Title = "Updated Book",
                Author = "Updated Author",
                CategoryId = categoryId,
                PublishDate = new DateOnly(2023, 5, 5),
                Quantity = 10,
                AvailableQuantity = 15 // Invalid: available > quantity
            };

            _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
                .ReturnsAsync(_books[0]);
            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync(_category);

            // Act
            var result = await _bookService.UpdateAsync(bookId, bookRequest);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Available can't bigger quantity"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));

            _mockBookRepository.Verify(repo => repo.Update(It.IsAny<Book>()), Times.Never);
            _mockBookRepository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public async Task DeleteByIdAsync_ShouldDeleteBook_WhenBookExistsAndIsFullyAvailable()
        {
            // Arrange
            // Book 2 has Quantity = AvailableQuantity = 5
            var bookId = _books[1].Id;

            _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
                .ReturnsAsync(_books[1]);

            // Act
            var result = await _bookService.DeleteByIdAsync(bookId);

            // Assert
            Assert.That(result.IsSuccess, Is.True);

            _mockBookRepository.Verify(repo => repo.Delete(It.IsAny<Book>()), Times.Once);
            _mockBookRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task DeleteByIdAsync_ShouldReturnFailure_WhenBookDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            _mockBookRepository.Setup(repo => repo.GetByIdAsync(nonExistentId))
                .ReturnsAsync((Book)null);

            // Act
            var result = await _bookService.DeleteByIdAsync(nonExistentId);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Book not found"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));

            _mockBookRepository.Verify(repo => repo.Delete(It.IsAny<Book>()), Times.Never);
            _mockBookRepository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public async Task DeleteByIdAsync_ShouldReturnFailure_WhenBookIsBeingBorrowed()
        {
            // Arrange
            // Book 1 has Quantity = 10, AvailableQuantity = 8 (some books are borrowed)
            var bookId = _books[0].Id;

            _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
                .ReturnsAsync(_books[0]);

            // Act
            var result = await _bookService.DeleteByIdAsync(bookId);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Cannot delete book when borrowing"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));

            _mockBookRepository.Verify(repo => repo.Delete(It.IsAny<Book>()), Times.Never);
            _mockBookRepository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }
    }
}
