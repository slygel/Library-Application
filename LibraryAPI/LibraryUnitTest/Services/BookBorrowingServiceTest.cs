using LibraryAPI.DTOs.BorrowingDto;
using LibraryAPI.Entities;
using LibraryAPI.Enums;
using LibraryAPI.IRepository;
using LibraryAPI.Services;
using Microsoft.AspNetCore.Http;
using Moq;

namespace LibraryUnitTest.Services
{
    class BookBorrowingServiceTest
    {
        private Mock<IBookBorrowingRepository> _mockBookBorrowingRepository;
        private Mock<IBookRepository> _mockBookRepository;
        private Mock<IUserRepository> _mockUserRepository;
        private BookBorrowingService _bookBorrowingService;
        private User _testUser;
        private User _adminUser;
        private List<Book> _books;
        private List<BookBorrowingRequest> _borrowingRequests;

        [SetUp]
        public void Setup()
        {
            _mockBookBorrowingRepository = new Mock<IBookBorrowingRepository>();
            _mockBookRepository = new Mock<IBookRepository>();
            _mockUserRepository = new Mock<IUserRepository>();

            _bookBorrowingService = new BookBorrowingService(
                _mockBookBorrowingRepository.Object,
                _mockBookRepository.Object,
                _mockUserRepository.Object);

            // Setup test user
            _testUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Password = "password123",
                Email = "test@example.com",
                Name = "Test User",
                Role = Role.User
            };

            // Setup admin user
            _adminUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                Password = "adminpassword",
                Email = "admin@example.com",
                Name = "Admin User",
                Role = Role.Admin
            };

            // Setup test books
            _books = new List<Book>
            {
                new Book
                {
                    Id = Guid.NewGuid(),
                    Title = "Book 1",
                    Author = "Author 1",
                    Description = "Description 1",
                    Quantity = 5,
                    AvailableQuantity = 3,
                    CategoryId = Guid.NewGuid()
                },
                new Book
                {
                    Id = Guid.NewGuid(),
                    Title = "Book 2",
                    Author = "Author 2",
                    Description = "Description 2",
                    Quantity = 3,
                    AvailableQuantity = 1,
                    CategoryId = Guid.NewGuid()
                },
                new Book
                {
                    Id = Guid.NewGuid(),
                    Title = "Book 3",
                    Author = "Author 3",
                    Description = "Description 3",
                    Quantity = 2,
                    AvailableQuantity = 0, // No available copies
                    CategoryId = Guid.NewGuid()
                }
            };

            // Setup test borrowing requests
            _borrowingRequests = new List<BookBorrowingRequest>
            {
                new BookBorrowingRequest
                {
                    Id = Guid.NewGuid(),
                    RequestDate = DateTime.UtcNow.AddDays(-5),
                    ExpirationDate = DateTime.UtcNow.AddDays(25),
                    Status = Status.Waiting,
                    ApproverId = _adminUser.Id,
                    RequestorId = _testUser.Id,
                    BookBorrowingRequestDetails = new List<BookBorrowingRequestDetails>
                    {
                        new BookBorrowingRequestDetails
                        {
                            Id = Guid.NewGuid(),
                            BookBorrowingRequestId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                            BookId = _books[0].Id
                        }
                    }
                },
                new BookBorrowingRequest
                {
                    Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                    RequestDate = DateTime.UtcNow.AddDays(-10),
                    ExpirationDate = DateTime.UtcNow.AddDays(20),
                    Status = Status.Approved,
                    ApproverId = _adminUser.Id,
                    RequestorId = _testUser.Id,
                    BookBorrowingRequestDetails = new List<BookBorrowingRequestDetails>
                    {
                        new BookBorrowingRequestDetails
                        {
                            Id = Guid.NewGuid(),
                            BookBorrowingRequestId = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                            BookId = _books[1].Id
                        }
                    }
                }
            };
        }

        [Test]
        public async Task GetUserBorrowingRequestsAsync_ShouldReturnFailure_WhenUserDoesNotExist()
        {
            // Arrange
            var nonExistentUserId = Guid.NewGuid();
            _mockUserRepository.Setup(repo => repo.GetByIdAsync(nonExistentUserId)).ReturnsAsync((User)null);

            // Act
            var result = await _bookBorrowingService.GetUserBorrowingRequestsAsync(nonExistentUserId, 1, 10);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("User not found"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task CreateBorrowingRequestAsync_ShouldCreateRequest_WhenRequestIsValid()
        {
            // Arrange
            var userId = _testUser.Id;
            var borrowingRequest = new BorrowingRequest
            {
                BookIds = new List<Guid> { _books[0].Id, _books[1].Id }
            };

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(_testUser);
            _mockUserRepository.Setup(repo => repo.GetByUsernameAsync("admin")).ReturnsAsync(_adminUser);
            _mockBookBorrowingRepository.Setup(repo => repo.GetCountForUserInCurrentMonthAsync(userId)).ReturnsAsync(2);

            _mockBookRepository.Setup(repo => repo.GetByIdAsync(_books[0].Id)).ReturnsAsync(_books[0]);
            _mockBookRepository.Setup(repo => repo.GetByIdAsync(_books[1].Id)).ReturnsAsync(_books[1]);

            _mockBookBorrowingRepository.Setup(repo => repo.CreateAsync(It.IsAny<BookBorrowingRequest>()))
                .ReturnsAsync((BookBorrowingRequest r) => r);

            // Act
            var result = await _bookBorrowingService.CreateBorrowingRequestAsync(borrowingRequest, userId);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.Books.Count, Is.EqualTo(2));

            _mockBookBorrowingRepository.Verify(repo => repo.CreateAsync(It.IsAny<BookBorrowingRequest>()), Times.Once);
            _mockBookBorrowingRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
            _mockBookRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task CreateBorrowingRequestAsync_ShouldReturnFailure_WhenUserDoesNotExist()
        {
            // Arrange
            var nonExistentUserId = Guid.NewGuid();
            var borrowingRequest = new BorrowingRequest
            {
                BookIds = new List<Guid> { _books[0].Id }
            };

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(nonExistentUserId)).ReturnsAsync((User)null);

            // Act
            var result = await _bookBorrowingService.CreateBorrowingRequestAsync(borrowingRequest, nonExistentUserId);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("User not found"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task CreateBorrowingRequestAsync_ShouldReturnFailure_WhenMonthlyLimitExceeded()
        {
            // Arrange
            var userId = _testUser.Id;
            var borrowingRequest = new BorrowingRequest
            {
                BookIds = new List<Guid> { _books[0].Id }
            };

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(_testUser);
            _mockBookBorrowingRepository.Setup(repo => repo.GetCountForUserInCurrentMonthAsync(userId)).ReturnsAsync(3); // Monthly limit reached

            // Act
            var result = await _bookBorrowingService.CreateBorrowingRequestAsync(borrowingRequest, userId);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("You have reached the maximum limit of 3 borrowing requests per month"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task CreateBorrowingRequestAsync_ShouldReturnFailure_WhenTooManyBooksRequested()
        {
            // Arrange
            var userId = _testUser.Id;
            var borrowingRequest = new BorrowingRequest
            {
                BookIds = new List<Guid>
            {
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() // 6 books, exceeding the 5 limit
            }
            };

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(_testUser);
            _mockBookBorrowingRepository.Setup(repo => repo.GetCountForUserInCurrentMonthAsync(userId)).ReturnsAsync(2);

            // Act
            var result = await _bookBorrowingService.CreateBorrowingRequestAsync(borrowingRequest, userId);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("You can borrow a maximum of 5 books per request"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task CreateBorrowingRequestAsync_ShouldReturnFailure_WhenNoBooksRequested()
        {
            // Arrange
            var userId = _testUser.Id;
            var borrowingRequest = new BorrowingRequest
            {
                BookIds = new List<Guid>() // Empty list
            };

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(_testUser);
            _mockBookBorrowingRepository.Setup(repo => repo.GetCountForUserInCurrentMonthAsync(userId)).ReturnsAsync(2);

            // Act
            var result = await _bookBorrowingService.CreateBorrowingRequestAsync(borrowingRequest, userId);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("You must specify at least one book to borrow"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task CreateBorrowingRequestAsync_ShouldReturnFailure_WhenBookDoesNotExist()
        {
            // Arrange
            var userId = _testUser.Id;
            var nonExistentBookId = Guid.NewGuid();
            var borrowingRequest = new BorrowingRequest
            {
                BookIds = new List<Guid> { nonExistentBookId }
            };

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(_testUser);
            _mockBookBorrowingRepository.Setup(repo => repo.GetCountForUserInCurrentMonthAsync(userId)).ReturnsAsync(2);
            _mockBookRepository.Setup(repo => repo.GetByIdAsync(nonExistentBookId)).ReturnsAsync((Book)null);

            // Act
            var result = await _bookBorrowingService.CreateBorrowingRequestAsync(borrowingRequest, userId);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo($"Book with ID {nonExistentBookId} not found"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task CreateBorrowingRequestAsync_ShouldReturnFailure_WhenBookIsNotAvailable()
        {
            // Arrange
            var userId = _testUser.Id;
            var unavailableBookId = _books[2].Id; // Book with 0 available quantity
            var borrowingRequest = new BorrowingRequest
            {
                BookIds = new List<Guid> { unavailableBookId }
            };

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(_testUser);
            _mockBookBorrowingRepository.Setup(repo => repo.GetCountForUserInCurrentMonthAsync(userId)).ReturnsAsync(2);
            _mockBookRepository.Setup(repo => repo.GetByIdAsync(unavailableBookId)).ReturnsAsync(_books[2]);

            // Act
            var result = await _bookBorrowingService.CreateBorrowingRequestAsync(borrowingRequest, userId);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo($"Book with ID {unavailableBookId} is not available for borrowing"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task CreateBorrowingRequestAsync_ShouldReturnFailure_WhenAdminNotFound()
        {
            // Arrange
            var userId = _testUser.Id;
            var borrowingRequest = new BorrowingRequest
            {
                BookIds = new List<Guid> { _books[0].Id }
            };

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(_testUser);
            _mockBookBorrowingRepository.Setup(repo => repo.GetCountForUserInCurrentMonthAsync(userId)).ReturnsAsync(2);
            _mockBookRepository.Setup(repo => repo.GetByIdAsync(_books[0].Id)).ReturnsAsync(_books[0]);
            _mockUserRepository.Setup(repo => repo.GetByUsernameAsync("admin")).ReturnsAsync((User)null);

            // Act
            var result = await _bookBorrowingService.CreateBorrowingRequestAsync(borrowingRequest, userId);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Admin account not found"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task UpdateBorrowingRequestStatusAsync_ShouldUpdateStatus_WhenRequestAndStatusAreValid()
        {
            // Arrange
            var adminId = _adminUser.Id;
            var borrowingRequestId = _borrowingRequests[0].Id; // A request in waiting status
            var newStatus = Status.Approved;

            _mockBookBorrowingRepository.Setup(repo => repo.GetByIdAsync(borrowingRequestId))
                .ReturnsAsync(_borrowingRequests[0]);
            _mockBookBorrowingRepository.Setup(repo => repo.UpdateStatus(It.IsAny<BookBorrowingRequest>()))
                .Returns((BookBorrowingRequest r) => r);

            // Act
            var result = await _bookBorrowingService.UpdateBorrowingRequestStatusAsync(borrowingRequestId, newStatus, adminId);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.Status, Is.EqualTo(newStatus));

            _mockBookBorrowingRepository.Verify(repo => repo.UpdateStatus(It.Is<BookBorrowingRequest>(r =>
                r.Status == newStatus && r.ApproverId == adminId)), Times.Once);
            _mockBookBorrowingRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task UpdateBorrowingRequestStatusAsync_ShouldReturnFailure_WhenRequestNotFound()
        {
            // Arrange
            var adminId = _adminUser.Id;
            var nonExistentRequestId = Guid.NewGuid();
            var newStatus = Status.Approved;

            _mockBookBorrowingRepository.Setup(repo => repo.GetByIdAsync(nonExistentRequestId))
                .ReturnsAsync((BookBorrowingRequest)null);

            // Act
            var result = await _bookBorrowingService.UpdateBorrowingRequestStatusAsync(nonExistentRequestId, newStatus, adminId);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo($"Borrowing request with ID {nonExistentRequestId} not found"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task UpdateBorrowingRequestStatusAsync_ShouldReturnFailure_WhenRequestStatusIsNotWaiting()
        {
            // Arrange
            var adminId = _adminUser.Id;
            var approvedRequestId = _borrowingRequests[1].Id; // A request already approved
            var newStatus = Status.Rejected;

            _mockBookBorrowingRepository.Setup(repo => repo.GetByIdAsync(approvedRequestId))
                .ReturnsAsync(_borrowingRequests[1]);

            // Act
            var result = await _bookBorrowingService.UpdateBorrowingRequestStatusAsync(approvedRequestId, newStatus, adminId);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo($"Cannot update status. The borrowing request is already {_borrowingRequests[1].Status}."));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task UpdateBorrowingRequestStatusAsync_ShouldIncreaseBookQuantity_WhenStatusIsRejected()
        {
            // Arrange
            var adminId = _adminUser.Id;
            var borrowingRequestId = _borrowingRequests[0].Id; // A request in waiting status
            var newStatus = Status.Rejected;

            _mockBookBorrowingRepository.Setup(repo => repo.GetByIdAsync(borrowingRequestId))
                .ReturnsAsync(_borrowingRequests[0]);
            _mockBookRepository.Setup(repo => repo.GetByIdAsync(_books[0].Id))
                .ReturnsAsync(_books[0]);
            _mockBookBorrowingRepository.Setup(repo => repo.UpdateStatus(It.IsAny<BookBorrowingRequest>()))
                .Returns((BookBorrowingRequest r) => r);

            // Act
            var result = await _bookBorrowingService.UpdateBorrowingRequestStatusAsync(borrowingRequestId, newStatus, adminId);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            _mockBookRepository.Verify(repo => repo.Update(It.Is<Book>(b =>
                b.Id == _books[0].Id && b.AvailableQuantity == 4)), Times.Once); // Increased from 3 to 4
            _mockBookRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task UpdateBorrowingRequestStatusAsync_ShouldReturnFailure_WhenBookNotFoundDuringRejection()
        {
            // Arrange
            var adminId = _adminUser.Id;
            var borrowingRequestId = _borrowingRequests[0].Id;
            var newStatus = Status.Rejected;

            // Create a copy of the borrowing request with a non-existent book ID
            var requestWithNonExistentBook = new BookBorrowingRequest
            {
                Id = borrowingRequestId,
                Status = Status.Waiting,
                BookBorrowingRequestDetails = new List<BookBorrowingRequestDetails>
            {
                new BookBorrowingRequestDetails
                {
                    BookId = Guid.NewGuid() // Non-existent book ID
                }
            }
            };

            _mockBookBorrowingRepository.Setup(repo => repo.GetByIdAsync(borrowingRequestId))
                .ReturnsAsync(requestWithNonExistentBook);
            _mockBookRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Book)null);

            // Act
            var result = await _bookBorrowingService.UpdateBorrowingRequestStatusAsync(borrowingRequestId, newStatus, adminId);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Contains.Substring("Book with ID"));
            Assert.That(result.ErrorMessage, Contains.Substring("not found"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }



        [Test]
        public async Task GetBorrowingRequestCountForUserInCurrentMonthAsync_ShouldReturnCount_WhenUserExists()
        {
            // Arrange
            var userId = _testUser.Id;
            var expectedCount = 2;

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(_testUser);
            _mockBookBorrowingRepository.Setup(repo => repo.GetCountForUserInCurrentMonthAsync(userId))
                .ReturnsAsync(expectedCount);

            // Act
            var result = await _bookBorrowingService.GetBorrowingRequestCountForUserInCurrentMonthAsync(userId);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(expectedCount));
        }

        [Test]
        public async Task GetBorrowingRequestCountForUserInCurrentMonthAsync_ShouldReturnFailure_WhenUserDoesNotExist()
        {
            // Arrange
            var nonExistentUserId = Guid.NewGuid();
            _mockUserRepository.Setup(repo => repo.GetByIdAsync(nonExistentUserId)).ReturnsAsync((User)null);

            // Act
            var result = await _bookBorrowingService.GetBorrowingRequestCountForUserInCurrentMonthAsync(nonExistentUserId);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("User not found"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }
    }
}
