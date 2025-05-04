using LibraryAPI.DbContext;
using LibraryAPI.Entities;
using LibraryAPI.Enums;
using LibraryAPI.Repository;
using Microsoft.EntityFrameworkCore;

namespace LibraryUnitTest.Repository
{
    class BookBorrowingRepositoryTest
    {
        private AppDbContext _context;
        private BookBorrowingRepository _repository;
        private User _requestor;
        private User _approver;
        private Category _category;
        private List<Book> _books;
        private List<BookBorrowingRequest> _borrowingRequests;


        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "LibraryDb_" + Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _repository = new BookBorrowingRepository(_context);

            // Create test users
            _requestor = new User
            {
                Id = Guid.NewGuid(),
                Name = "Requestor User",
                Email = "requestor@example.com",
                Username = "requestor",
                Password = "password",
                Role = Role.User
            };

            _approver = new User
            {
                Id = Guid.NewGuid(),
                Name = "Approver User",
                Email = "approver@example.com",
                Username = "approver",
                Password = "password",
                Role = Role.Admin
            };

            _context.Users.AddRange(new[] { _requestor, _approver });
            _context.SaveChanges();

            // Create test category
            _category = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Test Category"
            };
            _context.Categories.Add(_category);
            _context.SaveChanges();

            // Create test books
            _books = new List<Book>
        {
            new Book
            {
                Id = Guid.NewGuid(),
                Title = "Book 1",
                Author = "Author 1",
                CategoryId = _category.Id
            },
            new Book
            {
                Id = Guid.NewGuid(),
                Title = "Book 2",
                Author = "Author 2",
                CategoryId = _category.Id
            },
            new Book
            {
                Id = Guid.NewGuid(),
                Title = "Book 3",
                Author = "Author 3",
                CategoryId = _category.Id
            }
        };
            _context.Books.AddRange(_books);
            _context.SaveChanges();

            // Create test borrowing requests
            var today = DateTime.UtcNow;
            _borrowingRequests = new List<BookBorrowingRequest>
        {
            new BookBorrowingRequest
            {
                Id = Guid.NewGuid(),
                RequestDate = today.AddDays(-5),
                ExpirationDate = today.AddDays(25),
                Status = Status.Approved,
                RequestorId = _requestor.Id,
                ApproverId = _approver.Id,
                BookBorrowingRequestDetails = new List<BookBorrowingRequestDetails>
                {
                    new BookBorrowingRequestDetails
                    {
                        Id = Guid.NewGuid(),
                        BookId = _books[0].Id
                    }
                }
            },
            new BookBorrowingRequest
            {
                Id = Guid.NewGuid(),
                RequestDate = today.AddDays(-2),
                ExpirationDate = today.AddDays(28),
                Status = Status.Waiting,
                RequestorId = _requestor.Id,
                ApproverId = _approver.Id,
                BookBorrowingRequestDetails = new List<BookBorrowingRequestDetails>
                {
                    new BookBorrowingRequestDetails
                    {
                        Id = Guid.NewGuid(),
                        BookId = _books[1].Id
                    }
                }
            },
            new BookBorrowingRequest
            {
                Id = Guid.NewGuid(),
                RequestDate = today.AddDays(-10),
                ExpirationDate = today.AddDays(20),
                Status = Status.Rejected,
                RequestorId = _requestor.Id,
                ApproverId = _approver.Id,
                BookBorrowingRequestDetails = new List<BookBorrowingRequestDetails>
                {
                    new BookBorrowingRequestDetails
                    {
                        Id = Guid.NewGuid(),
                        BookId = _books[2].Id
                    }
                }
            }
        };
            _context.BookBorrowingRequests.AddRange(_borrowingRequests);
            _context.SaveChanges();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public void GetAll_ShouldReturnAllBorrowingRequests()
        {
            // Act
            var result = _repository.GetAll().ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result.Select(r => r.Id), Contains.Item(_borrowingRequests[0].Id));
            Assert.That(result.Select(r => r.Id), Contains.Item(_borrowingRequests[1].Id));
            Assert.That(result.Select(r => r.Id), Contains.Item(_borrowingRequests[2].Id));
        }

        [Test]
        public void GetAll_ShouldIncludeRelatedEntities()
        {
            // Act
            var result = _repository.GetAll().ToList();

            // Assert
            Assert.That(result.All(r => r.Requestor != null), Is.True);
            Assert.That(result.All(r => r.Approver != null), Is.True);
            Assert.That(result.All(r => r.BookBorrowingRequestDetails != null), Is.True);
            Assert.That(result.All(r => r.BookBorrowingRequestDetails.All(d => d.Book != null)), Is.True);
        }

        [Test]
        public void GetByUserId_ShouldReturnUserRequests()
        {
            // Act
            var result = _repository.GetByUserId(_requestor.Id).ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result.All(r => r.RequestorId == _requestor.Id), Is.True);
        }

        [Test]
        public void GetByUserId_ShouldReturnEmptyListWhenUserHasNoRequests()
        {
            // Arrange
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Name = "New User",
                Email = "newuser@example.com",
                Username = "newuser",
                Password = "password",
                Role = Role.User
            };
            _context.Users.Add(newUser);
            _context.SaveChanges();

            // Act
            var result = _repository.GetByUserId(newUser.Id).ToList();

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnRequest_WhenRequestExists()
        {
            // Arrange
            var existingRequest = _borrowingRequests[0];

            // Act
            var result = await _repository.GetByIdAsync(existingRequest.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(existingRequest.Id));
            Assert.That(result.Status, Is.EqualTo(existingRequest.Status));
            Assert.That(result.Requestor, Is.Not.Null);
            Assert.That(result.Approver, Is.Not.Null);
            Assert.That(result.BookBorrowingRequestDetails, Is.Not.Null);
            Assert.That(result.BookBorrowingRequestDetails.First().Book, Is.Not.Null);
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnNull_WhenRequestDoesNotExist()
        {
            // Act
            var result = await _repository.GetByIdAsync(Guid.NewGuid());

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetCountForUserInCurrentMonthAsync_ShouldReturnCorrectCount()
        {
            // Act
            var result = await _repository.GetCountForUserInCurrentMonthAsync(_requestor.Id);

            // Assert
            // Only counts non-rejected requests in current month
            var expectedCount = _borrowingRequests.Count(r =>
                r.RequestorId == _requestor.Id &&
                r.Status != Status.Rejected &&
                r.RequestDate.Month == DateTime.UtcNow.Month &&
                r.RequestDate.Year == DateTime.UtcNow.Year);

            Assert.That(result, Is.EqualTo(expectedCount));
        }

        [Test]
        public async Task CreateAsync_ShouldAddNewBorrowingRequest()
        {
            // Arrange
            var newRequest = new BookBorrowingRequest
            {
                Id = Guid.NewGuid(),
                RequestDate = DateTime.UtcNow,
                ExpirationDate = DateTime.UtcNow.AddDays(30),
                Status = Status.Waiting,
                RequestorId = _requestor.Id,
                ApproverId = _approver.Id,
                BookBorrowingRequestDetails = new List<BookBorrowingRequestDetails>
            {
                new BookBorrowingRequestDetails
                {
                    Id = Guid.NewGuid(),
                    BookId = _books[0].Id
                }
            }
            };

            // Act
            await _repository.CreateAsync(newRequest);
            await _repository.SaveChangesAsync();

            // Assert
            var savedRequest = await _context.BookBorrowingRequests
                .Include(r => r.BookBorrowingRequestDetails)
                .FirstOrDefaultAsync(r => r.Id == newRequest.Id);

            Assert.That(savedRequest, Is.Not.Null);
            Assert.That(savedRequest.Status, Is.EqualTo(Status.Waiting));
            Assert.That(savedRequest.RequestorId, Is.EqualTo(_requestor.Id));
            Assert.That(savedRequest.BookBorrowingRequestDetails, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task UpdateStatus_ShouldUpdateRequestStatus()
        {
            // Arrange
            var request = _borrowingRequests[1]; // Waiting request
            request.Status = Status.Approved;

            // Act
            _repository.UpdateStatus(request);
            await _repository.SaveChangesAsync();

            // Assert
            var updatedRequest = await _context.BookBorrowingRequests.FindAsync(request.Id);
            Assert.That(updatedRequest.Status, Is.EqualTo(Status.Approved));
        }

        [Test]
        public async Task SaveChangesAsync_ShouldPersistChanges()
        {
            // Arrange
            var request = _borrowingRequests[2]; // Rejected request
            request.ExpirationDate = request.ExpirationDate.AddDays(15);

            // Act
            await _repository.SaveChangesAsync();

            // Assert
            var updatedRequest = await _context.BookBorrowingRequests.FindAsync(request.Id);
            Assert.That(updatedRequest.ExpirationDate, Is.EqualTo(request.ExpirationDate));
        }
    }
}
