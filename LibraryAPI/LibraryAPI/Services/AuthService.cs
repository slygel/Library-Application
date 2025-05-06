using LibraryAPI.DTOs.AccountDto;
using LibraryAPI.Entities;
using LibraryAPI.Enums;
using LibraryAPI.Exceptions;
using LibraryAPI.Helpers;
using LibraryAPI.IRepository;
using LibraryAPI.IServices;

namespace LibraryAPI.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    
    public AuthService(ITokenService tokenService, IUserRepository userRepository)
    {
        _tokenService = tokenService;
        _userRepository = userRepository;
    }
    
    public async Task<Result<LoginResponse?>> Login(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Result<LoginResponse?>.Failure(
                "Username or password cannot be empty.", 
                StatusCodes.Status400BadRequest
            );
        }
        
        var user = await _userRepository.GetByUsernameAsync(request.Username);
        if (user == null)
        {
            return Result<LoginResponse?>.Failure(
                "User not found.",
                StatusCodes.Status400BadRequest
            );
        }
        
        if (string.IsNullOrEmpty(user.Password) || !PasswordHashHandler.VerifyPassword(request.Password, user.Password))
        {
            return Result<LoginResponse?>.Failure(
                "Invalid password.",
                StatusCodes.Status400BadRequest
            );
        }
    
        var accessToken = _tokenService.GenerateToken(user);
        var refreshToken = await _tokenService.CreateRefreshTokenAsync(user);
        
        return Result<LoginResponse?>.Success(new LoginResponse(accessToken, refreshToken.Token));
    }
    
    public async Task<Result> Register(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || 
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.Email) || 
            string.IsNullOrWhiteSpace(request.ConfirmPassword) || 
            string.IsNullOrWhiteSpace(request.Name) || 
            string.IsNullOrWhiteSpace(request.PhoneNumber) || 
            string.IsNullOrWhiteSpace(request.Address))
        {
            return Result.Failure("Please provide valid data.", StatusCodes.Status400BadRequest);
        }
        
        var username = await _userRepository.GetByUsernameAsync(request.Username);
        
        if (username != null)
        {
            return Result.Failure("Username existed", StatusCodes.Status400BadRequest);
        }
        
        var email = await _userRepository.GetByEmailAsync(request.Email);
        if (email != null)
        {
            return Result.Failure("Email existed", StatusCodes.Status400BadRequest);
        }
        
        // Validate password confirmation
        if (request.Password != request.ConfirmPassword)
        {
            return Result.Failure("Passwords do not match", StatusCodes.Status400BadRequest);
        }
        
        // Validate email format (basic check)
        if (!ValidEmail.IsValidEmail(request.Email))
        {
            return Result.Failure("Invalid email format", StatusCodes.Status400BadRequest);
        }
    
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Username = request.Username,
            Password = PasswordHashHandler.HashPassword(request.Password),
            Address = request.Address,
            Role = Role.User
        };
        try
        {
            await _userRepository.CreateAsync(user);
            
            await _userRepository.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure("Registration failed", StatusCodes.Status500InternalServerError);
        }

    }

    public async Task<Result<LoginResponse?>> RefreshToken(RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Result<LoginResponse?>.Failure(
                "Refresh token is required", 
                StatusCodes.Status400BadRequest);
        }
        
        var result = await _tokenService.RefreshTokenAsync(request.RefreshToken);
        if (!result.IsSuccess)
        {
            return Result<LoginResponse?>.Failure(result.ErrorMessage, StatusCodes.Status400BadRequest);
        }
        
        var token = result.Value;
        
        return Result<LoginResponse?>.Success(token);
    }

    public async Task<Result> Logout(RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Result.Failure("Refresh token is required", StatusCodes.Status400BadRequest);
        }

        var storedToken = await _tokenService.GetRefreshTokenAsync(request.RefreshToken);
        if (storedToken == null)
        {
            return Result.Failure("Invalid refresh token", StatusCodes.Status400BadRequest);
        }

        // Revoke the refresh token
        storedToken.IsRevoked = true;
        
        await _tokenService.UpdateRefreshTokenAsync(storedToken);
        
        return Result.Success();
    }
}