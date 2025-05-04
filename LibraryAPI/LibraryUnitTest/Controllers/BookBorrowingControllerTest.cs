using LibraryAPI.Controllers;
using LibraryAPI.DTOs.BorrowingDto;
using LibraryAPI.DTOs.UserDto;
using LibraryAPI.Enums;
using LibraryAPI.Exceptions;
using LibraryAPI.Helpers;
using LibraryAPI.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace LibraryUnitTest.Controllers
{
    class BookBorrowingControllerTest
    {
        private Mock<IBookBorrowingService> _mockBookBorrowingService;
        private BookBorrowingController _controller;
        private BorrowingRequest _borrowingRequest;
        private RequestBorrowingResponse _requestBorrowingResponse;
        private PaginatedList<BorrowingResponse> _paginatedBorrowingResponses;
        private List<BorrowingResponse> _borrowingResponses;
        private Guid _userId;
        private Guid _requestId;

        [SetUp]
        public void Setup()
        {
            _mockBookBorrowingService = new Mock<IBookBorrowingService>();
            _userId = Guid.NewGuid();
            _requestId = Guid.NewGuid();

            // Setup test user claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, _userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // Create controller with mock user
            _controller = new BookBorrowingController(_mockBookBorrowingService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = claimsPrincipal
                    }
                }
            };

            // Setup test borrowing request
            _borrowingRequest = new BorrowingRequest
            {
                BookIds = new List<Guid>
                {
                    Guid.NewGuid(),
                    Guid.NewGuid()
                }
            };

            // Setup test borrowing detail responses
            var borrowingDetails = new List<BorrowingDetailResponse>
            {
                new BorrowingDetailResponse
                {
                    Id = Guid.NewGuid(),
                    BookId = Guid.NewGuid(),
                    BookTitle = "Book 1"
                },
                new BorrowingDetailResponse
                {
                    Id = Guid.NewGuid(),
                    BookId = Guid.NewGuid(),
                    BookTitle = "Book 2"
                }
            };

            // Setup test request borrowing response
            _requestBorrowingResponse = new RequestBorrowingResponse
            {
                Id = _requestId,
                RequestDate = DateTime.Now,
                ExpirationDate = DateTime.Now.AddDays(14),
                Status = Status.Waiting,
                Books = borrowingDetails
            };

            // Setup test borrowing responses
            _borrowingResponses = new List<BorrowingResponse>
            {
                new BorrowingResponse
                {
                    Id = _requestId,
                    RequestDate = DateTime.Now,
                    ExpirationDate = DateTime.Now.AddDays(14),
                    Status = Status.Waiting,
                    Requestor = new UserResponse
                    {
                        Id = _userId,
                        Name = "Test User",
                        Email = "test@example.com"
                    },
                    Books = borrowingDetails
                },
                new BorrowingResponse
                {
                    Id = Guid.NewGuid(),
                    RequestDate = DateTime.Now.AddDays(-1),
                    ExpirationDate = DateTime.Now.AddDays(13),
                    Status = Status.Approved,
                    Requestor = new UserResponse
                    {
                        Id = Guid.NewGuid(),
                        Name = "Another User",
                        Email = "another@example.com"
                    },
                    Books = borrowingDetails
                }
            };

            _paginatedBorrowingResponses = new PaginatedList<BorrowingResponse>(_borrowingResponses, 2, 1, 10);
        }


        [Test]
        public async Task GetAllBorrowingRequests_ShouldReturnOk_WithPaginatedBorrowingRequests()
        {
            // Arrange
            _mockBookBorrowingService.Setup(service => service.GetAllBorrowingRequestsAsync(1, 10))
                .ReturnsAsync(_paginatedBorrowingResponses);

            // Act
            var result = await _controller.GetAllBorrowingRequests(1, 10);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());

            var okResult = result as OkObjectResult;
            Assert.That(okResult!.Value, Is.EqualTo(_paginatedBorrowingResponses));
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }


        [Test]
        public async Task GetMyBorrowingRequests_ShouldReturnOk_WhenUserHasBorrowingRequests()
        {
            // Arrange
            var userBorrowings = new PaginatedList<BorrowingResponse>(
                _borrowingResponses.Where(b => b.Requestor!.Id == _userId).ToList(), 1, 1, 10);

            _mockBookBorrowingService.Setup(service => service.GetUserBorrowingRequestsAsync(_userId, 1, 10))
                .ReturnsAsync(Result<PaginatedList<BorrowingResponse>>.Success(userBorrowings));

            // Act
            var result = await _controller.GetMyBorrowingRequests(1, 10);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());

            var okResult = result as OkObjectResult;
            Assert.That(okResult!.Value, Is.EqualTo(userBorrowings));
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }

        [Test]
        public async Task GetMyBorrowingRequests_ShouldReturnNotFound_WhenUserHasNoBorrowingRequests()
        {
            // Arrange
            _mockBookBorrowingService.Setup(service => service.GetUserBorrowingRequestsAsync(_userId, 1, 10))
                .ReturnsAsync(Result<PaginatedList<BorrowingResponse>>.Failure("No borrowing requests found", StatusCodes.Status404NotFound));

            // Act
            var result = await _controller.GetMyBorrowingRequests(1, 10);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());

            var objectResult = result as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        }


        [Test]
        public async Task CreateBorrowingRequest_ShouldReturnOk_WhenRequestIsValid()
        {
            // Arrange
            _mockBookBorrowingService.Setup(service => service.CreateBorrowingRequestAsync(_borrowingRequest, _userId))
                .ReturnsAsync(Result<RequestBorrowingResponse>.Success(_requestBorrowingResponse));

            // Act
            var result = await _controller.CreateBorrowingRequest(_borrowingRequest);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());

            var okResult = result as OkObjectResult;
            Assert.That(okResult!.Value, Is.EqualTo(_requestBorrowingResponse));
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }

        [Test]
        public async Task CreateBorrowingRequest_ShouldReturnBadRequest_WhenBooksAreUnavailable()
        {
            // Arrange
            _mockBookBorrowingService.Setup(service => service.CreateBorrowingRequestAsync(_borrowingRequest, _userId))
                .ReturnsAsync(Result<RequestBorrowingResponse>.Failure("One or more books are not available for borrowing",
                    StatusCodes.Status400BadRequest));

            // Act
            var result = await _controller.CreateBorrowingRequest(_borrowingRequest);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());

            var objectResult = result as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }


        [Test]
        public async Task ApproveBorrowingRequest_ShouldReturnOk_WhenRequestExists()
        {
            // Arrange
            var approvedResponse = new RequestBorrowingResponse
            {
                Id = _requestId,
                RequestDate = _requestBorrowingResponse.RequestDate,
                ExpirationDate = _requestBorrowingResponse.ExpirationDate,
                Status = Status.Approved,
                Books = _requestBorrowingResponse.Books
            };

            _mockBookBorrowingService.Setup(service => service.UpdateBorrowingRequestStatusAsync(_requestId, Status.Approved, _userId))
                .ReturnsAsync(Result<RequestBorrowingResponse>.Success(approvedResponse));

            // Act
            var result = await _controller.ApproveBorrowingRequest(_requestId);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());

            var okResult = result as OkObjectResult;
            Assert.That(okResult!.Value, Is.EqualTo(approvedResponse));
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }

        [Test]
        public async Task ApproveBorrowingRequest_ShouldReturnNotFound_WhenRequestDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            _mockBookBorrowingService.Setup(service => service.UpdateBorrowingRequestStatusAsync(nonExistentId, Status.Approved, _userId))
                .ReturnsAsync(Result<RequestBorrowingResponse>.Failure("Borrowing request not found", StatusCodes.Status404NotFound));

            // Act
            var result = await _controller.ApproveBorrowingRequest(nonExistentId);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());

            var objectResult = result as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        }


        [Test]
        public async Task RejectBorrowingRequest_ShouldReturnOk_WhenRequestExists()
        {
            // Arrange
            var rejectedResponse = new RequestBorrowingResponse
            {
                Id = _requestId,
                RequestDate = _requestBorrowingResponse.RequestDate,
                ExpirationDate = _requestBorrowingResponse.ExpirationDate,
                Status = Status.Rejected,
                Books = _requestBorrowingResponse.Books
            };

            _mockBookBorrowingService.Setup(service => service.UpdateBorrowingRequestStatusAsync(_requestId, Status.Rejected, _userId))
                .ReturnsAsync(Result<RequestBorrowingResponse>.Success(rejectedResponse));

            // Act
            var result = await _controller.RejectBorrowingRequest(_requestId);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());

            var okResult = result as OkObjectResult;
            Assert.That(okResult!.Value, Is.EqualTo(rejectedResponse));
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }

        [Test]
        public async Task RejectBorrowingRequest_ShouldReturnNotFound_WhenRequestDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            _mockBookBorrowingService.Setup(service => service.UpdateBorrowingRequestStatusAsync(nonExistentId, Status.Rejected, _userId))
                .ReturnsAsync(Result<RequestBorrowingResponse>.Failure("Borrowing request not found", StatusCodes.Status404NotFound));

            // Act
            var result = await _controller.RejectBorrowingRequest(nonExistentId);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());

            var objectResult = result as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        }


        [Test]
        public async Task GetMyBorrowingRequestCountForUser_ShouldReturnOk_WithBorrowingCount()
        {
            // Arrange
            int borrowingCount = 3;
            _mockBookBorrowingService.Setup(service => service.GetBorrowingRequestCountForUserInCurrentMonthAsync(_userId))
                .ReturnsAsync(Result<int>.Success(borrowingCount));

            // Act
            var result = await _controller.GetMyBorrowingRequestCountForUser();

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());

            var okResult = result as OkObjectResult;
            Assert.That(okResult!.Value, Is.EqualTo(borrowingCount));
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }

        [Test]
        public async Task GetMyBorrowingRequestCountForUser_ShouldReturnInternalServerError_WhenServiceFails()
        {
            // Arrange
            _mockBookBorrowingService.Setup(service => service.GetBorrowingRequestCountForUserInCurrentMonthAsync(_userId))
                .ReturnsAsync(Result<int>.Failure("Failed to retrieve borrowing count", StatusCodes.Status500InternalServerError));

            // Act
            var result = await _controller.GetMyBorrowingRequestCountForUser();

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());

            var objectResult = result as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        }

    }
}
