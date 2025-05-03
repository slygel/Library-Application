using LibraryAPI.DTOs.AccountDto;
using LibraryAPI.Exceptions;
using LoginRequest = LibraryAPI.DTOs.AccountDto.LoginRequest;
using RegisterRequest = LibraryAPI.DTOs.AccountDto.RegisterRequest;

namespace LibraryAPI.IServices;

public interface IAuthService
{
    Task<Result<LoginResponse?>> Login(LoginRequest request);
    Task<Result> Register(RegisterRequest request);
    Task<Result<LoginResponse?>> RefreshToken(RefreshTokenRequest request);
    Task<Result> Logout(RefreshTokenRequest request);
}