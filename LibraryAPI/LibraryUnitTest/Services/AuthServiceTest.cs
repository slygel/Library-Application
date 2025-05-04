using LibraryAPI.DTOs.AccountDto;
using LibraryAPI.Entities;
using LibraryAPI.Enums;
using LibraryAPI.Exceptions;
using LibraryAPI.IRepository;
using LibraryAPI.IServices;
using LibraryAPI.Services;
using Microsoft.AspNetCore.Http;
using Moq;

namespace LibraryUnitTest.Services
{
    class AuthServiceTest
    {
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<ITokenService> _mockTokenService;
        private AuthService _authService;
        private User _testUser;
        private RefreshToken _testRefreshToken;
        private string _accessToken = "test-access-token";

        [SetUp]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockTokenService = new Mock<ITokenService>();
            _authService = new AuthService(_mockTokenService.Object, _mockUserRepository.Object);

            // Setup test user with hashed password
            string plainPassword = "Test123";
            string hashedPassword = PasswordHashHandler.HashPassword(plainPassword);

            _testUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Password = hashedPassword,
                Email = "test@example.com",
                Name = "Test User",
                PhoneNumber = "1234567890",
                Address = "Test Address",
                Role = Role.User
            };

            // Setup test refresh token
            _testRefreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = "valid-refresh-token",
                UserId = _testUser.Id,
                User = _testUser,
                CreatedAt = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                IsUsed = false
            };
        }

        [Test]
        public async Task Login_ShouldReturnSuccess_WhenCredentialsAreValid()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = _testUser.Username,
                Password = "Test123"
            };

            _mockUserRepository.Setup(repo => repo.GetByUsernameAsync(loginRequest.Username))
                .ReturnsAsync(_testUser);
            _mockTokenService.Setup(service => service.GenerateToken(_testUser))
                .Returns(_accessToken);
            _mockTokenService.Setup(service => service.CreateRefreshTokenAsync(_testUser))
                .ReturnsAsync(_testRefreshToken);

            // Act
            var result = await _authService.Login(loginRequest);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.AccessToken, Is.EqualTo(_accessToken));
            Assert.That(result.Value.RefreshToken, Is.EqualTo(_testRefreshToken.Token));
        }

        [Test]
        public async Task Login_ShouldReturnFailure_WhenUsernameOrPasswordIsEmpty()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "",
                Password = "Test123"
            };

            // Act
            var result = await _authService.Login(loginRequest);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Username or password cannot be empty."));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task Login_ShouldReturnFailure_WhenUserNotFound()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "nonexistentuser",
                Password = "Test123"
            };

            _mockUserRepository.Setup(repo => repo.GetByUsernameAsync(loginRequest.Username))
                .ReturnsAsync((User)null);

            // Act
            var result = await _authService.Login(loginRequest);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("User not found."));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task Login_ShouldReturnFailure_WhenPasswordIsInvalid()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = _testUser.Username,
                Password = "WrongPassword"
            };

            _mockUserRepository.Setup(repo => repo.GetByUsernameAsync(loginRequest.Username))
                .ReturnsAsync(_testUser);

            // Act
            var result = await _authService.Login(loginRequest);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Invalid password."));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task Register_ShouldReturnSuccess_WhenRequestIsValid()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "newuser",
                Password = "Password123",
                ConfirmPassword = "Password123",
                Email = "newuser@example.com",
                Name = "New User",
                PhoneNumber = "9876543210",
                Address = "New Address"
            };

            _mockUserRepository.Setup(repo => repo.GetByUsernameAsync(registerRequest.Username))
                .ReturnsAsync((User)null);
            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(registerRequest.Email))
                .ReturnsAsync((User)null);
            _mockUserRepository.Setup(repo => repo.CreateAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);
            _mockUserRepository.Setup(repo => repo.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.Register(registerRequest);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            _mockUserRepository.Verify(repo => repo.CreateAsync(It.Is<User>(u =>
                u.Username == registerRequest.Username &&
                u.Email == registerRequest.Email &&
                u.Role == Role.User)), Times.Once);
            _mockUserRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task Register_ShouldReturnFailure_WhenRequiredFieldsAreMissing()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "",
                Password = "Password123",
                ConfirmPassword = "Password123",
                Email = "newuser@example.com",
                Name = "New User",
                PhoneNumber = "9876543210",
                Address = "New Address"
            };

            // Act
            var result = await _authService.Register(registerRequest);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Please provide valid data."));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task Register_ShouldReturnFailure_WhenUsernameAlreadyExists()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "existinguser",
                Password = "Password123",
                ConfirmPassword = "Password123",
                Email = "newuser@example.com",
                Name = "New User",
                PhoneNumber = "9876543210",
                Address = "New Address"
            };

            _mockUserRepository.Setup(repo => repo.GetByUsernameAsync(registerRequest.Username))
                .ReturnsAsync(new User 
                {   Username = "existinguser",
                    Password = "Password123",
                    Email = "newuser@example.com",
                    Name = "New User",
                    PhoneNumber = "9876543210",
                    Address = "New Address"
                });

            // Act
            var result = await _authService.Register(registerRequest);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Username existed"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task Register_ShouldReturnFailure_WhenEmailAlreadyExists()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "newuser",
                Password = "Password123",
                ConfirmPassword = "Password123",
                Email = "existing@example.com",
                Name = "New User",
                PhoneNumber = "9876543210",
                Address = "New Address"
            };

            _mockUserRepository.Setup(repo => repo.GetByUsernameAsync(registerRequest.Username))
                .ReturnsAsync((User)null);
            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(registerRequest.Email))
                .ReturnsAsync(new User 
                {
                    Username = "existinguser",
                    Password = "Password123",
                    Name = "New User",
                    PhoneNumber = "9876543210",
                    Address = "New Address",
                    Email = "existing@example.com" 
                });

            // Act
            var result = await _authService.Register(registerRequest);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Email existed"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task Register_ShouldReturnFailure_WhenPasswordsDoNotMatch()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "newuser",
                Password = "Password123",
                ConfirmPassword = "DifferentPassword",
                Email = "newuser@example.com",
                Name = "New User",
                PhoneNumber = "9876543210",
                Address = "New Address"
            };

            _mockUserRepository.Setup(repo => repo.GetByUsernameAsync(registerRequest.Username))
                .ReturnsAsync((User)null);
            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(registerRequest.Email))
                .ReturnsAsync((User)null);

            // Act
            var result = await _authService.Register(registerRequest);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Passwords do not match"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task Register_ShouldReturnFailure_WhenEmailFormatIsInvalid()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "newuser",
                Password = "Password123",
                ConfirmPassword = "Password123",
                Email = "invalid-email",
                Name = "New User",
                PhoneNumber = "9876543210",
                Address = "New Address"
            };

            _mockUserRepository.Setup(repo => repo.GetByUsernameAsync(registerRequest.Username))
                .ReturnsAsync((User)null);
            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(registerRequest.Email))
                .ReturnsAsync((User)null);

            // Act
            var result = await _authService.Register(registerRequest);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Invalid email format"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task Register_ShouldReturnFailure_WhenExceptionOccurs()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "newuser",
                Password = "Password123",
                ConfirmPassword = "Password123",
                Email = "newuser@example.com",
                Name = "New User",
                PhoneNumber = "9876543210",
                Address = "New Address"
            };

            _mockUserRepository.Setup(repo => repo.GetByUsernameAsync(registerRequest.Username))
                .ReturnsAsync((User)null);
            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(registerRequest.Email))
                .ReturnsAsync((User)null);
            _mockUserRepository.Setup(repo => repo.CreateAsync(It.IsAny<User>()))
                .Throws(new Exception("Database error"));

            // Act
            var result = await _authService.Register(registerRequest);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Registration failed"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        }

        [Test]
        public async Task RefreshToken_ShouldReturnSuccess_WhenTokenIsValid()
        {
            // Arrange
            var refreshTokenRequest = new RefreshTokenRequest
            {
                RefreshToken = _testRefreshToken.Token
            };

            var loginResponse = new LoginResponse(_accessToken, "new-refresh-token");

            _mockTokenService.Setup(service => service.RefreshTokenAsync(refreshTokenRequest.RefreshToken))
                .ReturnsAsync(Result<LoginResponse>.Success(loginResponse));

            // Act
            var result = await _authService.RefreshToken(refreshTokenRequest);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.AccessToken, Is.EqualTo(_accessToken));
            Assert.That(result.Value.RefreshToken, Is.EqualTo("new-refresh-token"));
        }

        [Test]
        public async Task RefreshToken_ShouldReturnFailure_WhenRefreshTokenIsEmpty()
        {
            // Arrange
            var refreshTokenRequest = new RefreshTokenRequest
            {
                RefreshToken = ""
            };

            // Act
            var result = await _authService.RefreshToken(refreshTokenRequest);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Refresh token is required"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task RefreshToken_ShouldReturnFailure_WhenTokenServiceReturnFailure()
        {
            // Arrange
            var refreshTokenRequest = new RefreshTokenRequest
            {
                RefreshToken = "invalid-token"
            };

            _mockTokenService.Setup(service => service.RefreshTokenAsync(refreshTokenRequest.RefreshToken))
                .ReturnsAsync(Result<LoginResponse>.Failure("Token is invalid", StatusCodes.Status400BadRequest));

            // Act
            var result = await _authService.RefreshToken(refreshTokenRequest);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Token is invalid"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task Logout_ShouldReturnSuccess_WhenTokenIsValid()
        {
            // Arrange
            var logoutRequest = new RefreshTokenRequest
            {
                RefreshToken = _testRefreshToken.Token
            };

            _mockTokenService.Setup(service => service.GetRefreshTokenAsync(logoutRequest.RefreshToken))
                .ReturnsAsync(_testRefreshToken);
            _mockTokenService.Setup(service => service.UpdateRefreshTokenAsync(It.IsAny<RefreshToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.Logout(logoutRequest);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            _mockTokenService.Verify(service => service.UpdateRefreshTokenAsync(
                It.Is<RefreshToken>(rt => rt.IsRevoked == true)), Times.Once);
        }

        [Test]
        public async Task Logout_ShouldReturnFailure_WhenRefreshTokenIsEmpty()
        {
            // Arrange
            var logoutRequest = new RefreshTokenRequest
            {
                RefreshToken = ""
            };

            // Act
            var result = await _authService.Logout(logoutRequest);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Refresh token is required"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task Logout_ShouldReturnFailure_WhenTokenIsInvalid()
        {
            // Arrange
            var logoutRequest = new RefreshTokenRequest
            {
                RefreshToken = "invalid-token"
            };

            _mockTokenService.Setup(service => service.GetRefreshTokenAsync(logoutRequest.RefreshToken))
                .ReturnsAsync((RefreshToken)null);

            // Act
            var result = await _authService.Logout(logoutRequest);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Invalid refresh token"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }
    }
}
