using LibraryAPI.Controllers;
using LibraryAPI.DTOs.BookDto;
using LibraryAPI.Exceptions;
using LibraryAPI.Helpers;
using LibraryAPI.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LibraryUnitTest.Controllers
{
    class BookControllerTest
    {
        private Mock<IBookService> _mockBookService;
        private BookController _controller;
        private BookResponse _testBook;
        private List<BookResponse> _testBooks;
        private PaginatedList<BookResponse> _paginatedBooks;
        private Guid _categoryId;

        [SetUp]
        public void Setup()
        {
            _mockBookService = new Mock<IBookService>();
            _controller = new BookController(_mockBookService.Object);

            _categoryId = Guid.NewGuid();

            // Setup test book
            _testBook = new BookResponse
            {
                Id = Guid.NewGuid(),
                Title = "Test Book",
                Author = "Test Author",
                CategoryId = _categoryId,
                PublishDate = new DateOnly(2020, 1, 1),
                Isbn = "ISBN-TEST-123",
                Quantity = 10,
                AvailableQuantity = 5,
                Description = "Test Book Description"
            };

            // Setup test books list
            _testBooks = new List<BookResponse>
            {
                _testBook,
                new BookResponse
                {
                    Id = Guid.NewGuid(),
                    Title = "Fiction Book",
                    Author = "Fiction Author",
                    CategoryId = _categoryId,
                    PublishDate = new DateOnly(2021, 2, 15),
                    Isbn = "ISBN-FICTION-456",
                    Quantity = 15,
                    AvailableQuantity = 10,
                    Description = "Fiction Book Description"
                },
                new BookResponse
                {
                    Id = Guid.NewGuid(),
                    Title = "Non-Fiction Book",
                    Author = "Non-Fiction Author",
                    CategoryId = _categoryId,
                    PublishDate = new DateOnly(2022, 3, 30),
                    Isbn = "ISBN-NONFICTION-789",
                    Quantity = 8,
                    AvailableQuantity = 8,
                    Description = "Non-Fiction Book Description"
                }
            };

            _paginatedBooks = new PaginatedList<BookResponse>(_testBooks, 3, 1, 10);
        }


        [Test]
        public async Task GetAllAsync_ShouldReturnOk_WithPaginatedBooks()
        {
            // Arrange
            _mockBookService.Setup(service => service.GetAllAsync(1, 10, null, null))
                .ReturnsAsync(_paginatedBooks);

            // Act
            var result = await _controller.GetAllAsync(1, 10);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());

            var okResult = result as OkObjectResult;
            Assert.That(okResult!.Value, Is.EqualTo(_paginatedBooks));
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnOk_WithFilteredBooks_WhenBookTitleProvided()
        {
            // Arrange
            var bookTitle = "Fiction";
            var filteredBooks = _testBooks.Where(b => b.Title.Contains(bookTitle)).ToList();
            var paginatedFilteredBooks = new PaginatedList<BookResponse>(filteredBooks, filteredBooks.Count, 1, 10);

            _mockBookService.Setup(service => service.GetAllAsync(1, 10, bookTitle, null))
                .ReturnsAsync(paginatedFilteredBooks);

            // Act
            var result = await _controller.GetAllAsync(1, 10, bookTitle);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());

            var okResult = result as OkObjectResult;
            Assert.That(okResult!.Value, Is.EqualTo(paginatedFilteredBooks));
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnOk_WithFilteredBooks_WhenCategoryIdProvided()
        {
            // Arrange
            var filteredBooks = _testBooks.Where(b => b.CategoryId == _categoryId).ToList();
            var paginatedFilteredBooks = new PaginatedList<BookResponse>(filteredBooks, filteredBooks.Count, 1, 10);

            _mockBookService.Setup(service => service.GetAllAsync(1, 10, null, _categoryId))
                .ReturnsAsync(paginatedFilteredBooks);

            // Act
            var result = await _controller.GetAllAsync(1, 10, null, _categoryId);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());

            var okResult = result as OkObjectResult;
            Assert.That(okResult!.Value, Is.EqualTo(paginatedFilteredBooks));
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }

        [Test]
        public async Task CreateAsync_ShouldReturnOk_WhenRequestIsValid()
        {
            // Arrange
            var bookRequest = new BookRequest
            {
                Title = "New Book",
                Author = "New Author",
                CategoryId = _categoryId,
                PublishDate = new DateOnly(2023, 5, 15),
                Isbn = "ISBN-NEW-123",
                Quantity = 5,
                AvailableQuantity = 5,
                Description = "New Book Description"
            };

            var newBook = new BookResponse
            {
                Id = Guid.NewGuid(),
                Title = bookRequest.Title,
                Author = bookRequest.Author,
                CategoryId = bookRequest.CategoryId,
                PublishDate = bookRequest.PublishDate,
                Isbn = bookRequest.Isbn,
                Quantity = bookRequest.Quantity,
                AvailableQuantity = bookRequest.AvailableQuantity,
                Description = bookRequest.Description
            };

            _mockBookService.Setup(service => service.CreateAsync(bookRequest))
                .ReturnsAsync(Result<BookResponse>.Success(newBook));

            // Act
            var result = await _controller.CreateAsync(bookRequest);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());

            var okResult = result as OkObjectResult;
            Assert.That(okResult!.Value, Is.EqualTo(newBook));
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }

        [Test]
        public async Task CreateAsync_ShouldReturnBadRequest_WhenRequestIsInvalid()
        {
            // Arrange
            var invalidRequest = new BookRequest
            {
                Title = "Invalid Book",
                Author = "Invalid Author",
                CategoryId = Guid.NewGuid(), // Non-existent category
                PublishDate = new DateOnly(2023, 5, 15),
                Isbn = "ISBN-INVALID-123",
                Quantity = 5,
                AvailableQuantity = 5,
                Description = "Invalid Book Description"
            };

            _mockBookService.Setup(service => service.CreateAsync(invalidRequest))
                .ReturnsAsync(Result<BookResponse>.Failure("Category not found", StatusCodes.Status400BadRequest));

            // Act
            var result = await _controller.CreateAsync(invalidRequest);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());

            var objectResult = result as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }


        [Test]
        public async Task UpdateAsync_ShouldReturnOk_WhenRequestIsValid()
        {
            // Arrange
            var bookId = _testBook.Id;
            var updateRequest = new BookRequest
            {
                Title = "Updated Book",
                Author = "Updated Author",
                CategoryId = _categoryId,
                PublishDate = new DateOnly(2023, 6, 20),
                Isbn = "ISBN-UPDATED-123",
                Quantity = 8,
                AvailableQuantity = 3,
                Description = "Updated Book Description"
            };

            var updatedBook = new BookResponse
            {
                Id = bookId,
                Title = updateRequest.Title,
                Author = updateRequest.Author,
                CategoryId = updateRequest.CategoryId,
                PublishDate = updateRequest.PublishDate,
                Isbn = updateRequest.Isbn,
                Quantity = updateRequest.Quantity,
                AvailableQuantity = updateRequest.AvailableQuantity,
                Description = updateRequest.Description
            };

            _mockBookService.Setup(service => service.UpdateAsync(bookId, updateRequest))
                .ReturnsAsync(Result<BookResponse>.Success(updatedBook));

            // Act
            var result = await _controller.UpdateAsync(bookId, updateRequest);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());

            var okResult = result as OkObjectResult;
            Assert.That(okResult!.Value, Is.EqualTo(updatedBook));
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }

        [Test]
        public async Task UpdateAsync_ShouldReturnNotFound_WhenBookDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var updateRequest = new BookRequest
            {
                Title = "Updated Book",
                Author = "Updated Author",
                CategoryId = _categoryId,
                PublishDate = new DateOnly(2023, 6, 20),
                Isbn = "ISBN-UPDATED-123",
                Quantity = 8,
                AvailableQuantity = 3,
                Description = "Updated Book Description"
            };

            _mockBookService.Setup(service => service.UpdateAsync(nonExistentId, updateRequest))
                .ReturnsAsync(Result<BookResponse>.Failure("Book not found", StatusCodes.Status404NotFound));

            // Act
            var result = await _controller.UpdateAsync(nonExistentId, updateRequest);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());

            var objectResult = result as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        }


        [Test]
        public async Task DeleteByIdAsync_ShouldReturnOk_WhenBookExists()
        {
            // Arrange
            var bookId = _testBook.Id;
            _mockBookService.Setup(service => service.DeleteByIdAsync(bookId))
                .ReturnsAsync(Result.Success());

            // Act
            var result = await _controller.DeleteByIdAsync(bookId);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());

            var okResult = result as OkObjectResult;
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }

        [Test]
        public async Task DeleteByIdAsync_ShouldReturnNotFound_WhenBookDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            _mockBookService.Setup(service => service.DeleteByIdAsync(nonExistentId))
                .ReturnsAsync(Result.Failure("Book not found", StatusCodes.Status404NotFound));

            // Act
            var result = await _controller.DeleteByIdAsync(nonExistentId);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());

            var objectResult = result as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        }

        [Test]
        public async Task DeleteByIdAsync_ShouldReturnBadRequest_WhenBookIsBeingBorrowed()
        {
            // Arrange
            var bookId = _testBook.Id;
            _mockBookService.Setup(service => service.DeleteByIdAsync(bookId))
                .ReturnsAsync(Result.Failure("Book cannot be deleted because it is currently being borrowed", StatusCodes.Status400BadRequest));

            // Act
            var result = await _controller.DeleteByIdAsync(bookId);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());

            var objectResult = result as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }
    }
}
