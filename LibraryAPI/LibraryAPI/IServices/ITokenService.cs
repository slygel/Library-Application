using LibraryAPI.DTOs.AccountDto;
using LibraryAPI.Entities;
using LibraryAPI.Exceptions;

namespace LibraryAPI.IServices;

public interface ITokenService
{
    string GenerateToken(User? account);
    Task<RefreshToken> CreateRefreshTokenAsync(User user);
    Task<Result<LoginResponse>> RefreshTokenAsync(string refreshToken);
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
    Task UpdateRefreshTokenAsync(RefreshToken refreshToken);
}