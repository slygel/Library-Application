using LibraryAPI.Controllers;
using LibraryAPI.DTOs.AccountDto;
using LibraryAPI.Exceptions;
using LibraryAPI.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LibraryUnitTest.Controllers
{
    class AuthControllerTest
    {
        private Mock<IAuthService> _mockAuthService;
        private AuthController _controller;
        private LoginRequest _loginRequest;
        private RegisterRequest _registerRequest;
        private RefreshTokenRequest _refreshTokenRequest;
        private LoginResponse _loginResponse;

        [SetUp]
        public void Setup()
        {
            _mockAuthService = new Mock<IAuthService>();
            _controller = new AuthController(_mockAuthService.Object);

            // Setup test login request
            _loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = "Password123!"
            };

            // Setup test register request
            _registerRequest = new RegisterRequest
            {
                Name = "Test User",
                Username = "testuser",
                Email = "test@example.com",
                PhoneNumber = "1234567890",
                Address = "123 Test St",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            // Setup test refresh token request
            _refreshTokenRequest = new RefreshTokenRequest
            {
                RefreshToken = "test-refresh-token"
            };

            // Setup test login response
            _loginResponse = new LoginResponse
            {
                AccessToken = "test-access-token",
                RefreshToken = "test-refresh-token"
            };
        }


        [Test]
        public async Task Login_ShouldReturnOk_WhenLoginIsSuccessful()
        {
            // Arrange
            _mockAuthService.Setup(service => service.Login(_loginRequest))
                .ReturnsAsync(Result<LoginResponse>.Success(_loginResponse));

            // Act
            var result = await _controller.Login(_loginRequest);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());

            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult!.Value, Is.EqualTo(_loginResponse));
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));

            var loginResponse = okResult.Value as LoginResponse;
            Assert.That(loginResponse, Is.Not.Null);
            Assert.That(loginResponse!.AccessToken, Is.EqualTo(_loginResponse.AccessToken));
            Assert.That(loginResponse.RefreshToken, Is.EqualTo(_loginResponse.RefreshToken));
        }

        [Test]
        public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
        {
            // Arrange
            _mockAuthService.Setup(service => service.Login(_loginRequest))
                .ReturnsAsync(Result<LoginResponse>.Failure("Invalid username or password", StatusCodes.Status401Unauthorized));

            // Act
            var result = await _controller.Login(_loginRequest);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());

            var objectResult = result.Result as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
        }


        [Test]
        public async Task Register_ShouldReturnOk_WhenRegistrationIsSuccessful()
        {
            // Arrange
            var successResult = Result.Success();
            _mockAuthService.Setup(service => service.Register(_registerRequest))
                .ReturnsAsync(successResult);

            // Act
            var result = await _controller.Register(_registerRequest);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());

            var okResult = result as OkObjectResult;
            Assert.That(okResult!.Value, Is.EqualTo(successResult));
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }

        [Test]
        public async Task Register_ShouldReturnBadRequest_WhenRegistrationFails()
        {
            // Arrange
            _mockAuthService.Setup(service => service.Register(_registerRequest))
                .ReturnsAsync(Result.Failure("Username already exists", StatusCodes.Status400BadRequest));

            // Act
            var result = await _controller.Register(_registerRequest);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());

            var objectResult = result as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }


        [Test]
        public async Task RefreshToken_ShouldReturnOk_WhenRefreshIsSuccessful()
        {
            // Arrange
            _mockAuthService.Setup(service => service.RefreshToken(_refreshTokenRequest))
                .ReturnsAsync(Result<LoginResponse>.Success(_loginResponse));

            // Act
            var result = await _controller.RefreshToken(_refreshTokenRequest);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());

            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult!.Value, Is.EqualTo(_loginResponse));
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));

            var loginResponse = okResult.Value as LoginResponse;
            Assert.That(loginResponse, Is.Not.Null);
            Assert.That(loginResponse!.AccessToken, Is.EqualTo(_loginResponse.AccessToken));
            Assert.That(loginResponse.RefreshToken, Is.EqualTo(_loginResponse.RefreshToken));
        }

        [Test]
        public async Task RefreshToken_ShouldReturnUnauthorized_WhenRefreshTokenIsInvalid()
        {
            // Arrange
            _mockAuthService.Setup(service => service.RefreshToken(_refreshTokenRequest))
                .ReturnsAsync(Result<LoginResponse>.Failure("Invalid refresh token", StatusCodes.Status401Unauthorized));

            // Act
            var result = await _controller.RefreshToken(_refreshTokenRequest);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());

            var objectResult = result.Result as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
        }


        [Test]
        public async Task Logout_ShouldReturnOk_WhenLogoutIsSuccessful()
        {
            // Arrange
            var successResult = Result.Success();
            _mockAuthService.Setup(service => service.Logout(_refreshTokenRequest))
                .ReturnsAsync(successResult);

            // Act
            var result = await _controller.Logout(_refreshTokenRequest);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());

            var okResult = result as OkObjectResult;
            Assert.That(okResult!.Value, Is.EqualTo(successResult));
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }

        [Test]
        public async Task Logout_ShouldReturnBadRequest_WhenLogoutFails()
        {
            // Arrange
            _mockAuthService.Setup(service => service.Logout(_refreshTokenRequest))
                .ReturnsAsync(Result.Failure("Invalid refresh token", StatusCodes.Status400BadRequest));

            // Act
            var result = await _controller.Logout(_refreshTokenRequest);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());

            var objectResult = result as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));

        }
    }
}
