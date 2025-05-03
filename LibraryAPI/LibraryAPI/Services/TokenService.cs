using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LibraryAPI.DTOs.AccountDto;
using LibraryAPI.Entities;
using LibraryAPI.Exceptions;
using LibraryAPI.IRepository;
using LibraryAPI.IServices;
using Microsoft.IdentityModel.Tokens;

namespace LibraryAPI.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public TokenService(
        IConfiguration configuration,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _configuration = configuration;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public string GenerateToken(User? user)
    {
        var issuer = _configuration["JwtConfig:Issuer"];
        var audience = _configuration["JwtConfig:Audience"];
        var key = _configuration["JwtConfig:Key"];
        var tokenValidityMins = GetTokenValidityMinutes();
        
        var tokeDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(JwtRegisteredClaimNames.Sub, user?.Id.ToString() ?? string.Empty),
                new Claim(ClaimTypes.Role, user?.Role.ToString() ?? string.Empty),
                new Claim(ClaimTypes.Name, user?.Username ?? string.Empty)
            ]),
            Expires = DateTime.UtcNow.AddMinutes(tokenValidityMins),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
                SecurityAlgorithms.HmacSha256Signature),
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokeDescriptor);
        var accessToken = tokenHandler.WriteToken(securityToken);
    
        return accessToken;
    }
    
    private int GetTokenValidityMinutes()
    {
        return _configuration.GetValue<int>("JwtConfig:TokenValidityMins");
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public async Task<RefreshToken> CreateRefreshTokenAsync(User user)
    {
        var token = GenerateRefreshToken();
        var refreshTokenValidityDays = GetRefreshTokenValidityDays();
        
        // Revoke any existing active refresh tokens for this user
        await _refreshTokenRepository.RevokeAllUserTokensAsync(user.Id);
        
        // Create the new refresh token
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(refreshTokenValidityDays),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false,
            IsUsed = false
        };

        await _refreshTokenRepository.CreateAsync(refreshToken);
        
        await _refreshTokenRepository.SaveChangesAsync();

        return refreshToken;
    }

    private int GetRefreshTokenValidityDays()
    {
        return _configuration.GetValue<int>("JwtConfig:RefreshTokenValidityDays");
    }

    public async Task<Result<LoginResponse>> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
        
        if (storedToken == null)
        {
            return Result<LoginResponse>.Failure("Invalid refresh token", StatusCodes.Status400BadRequest);
        }

        if (storedToken.ExpiryDate < DateTime.UtcNow)
        {
            return Result<LoginResponse>.Failure("Refresh token expired", StatusCodes.Status400BadRequest);
        }

        if (storedToken.IsRevoked)
        {
            return Result<LoginResponse>.Failure("Refresh token revoked", StatusCodes.Status400BadRequest);
        }

        if (storedToken.IsUsed)
        {
            return Result<LoginResponse>.Failure("Refresh token already used", StatusCodes.Status400BadRequest);
        }

        // Mark the current token as used
        storedToken.IsUsed = true;
        _refreshTokenRepository.Update(storedToken);
        
        // Generate a new access token
        var user = storedToken.User;
        var accessToken = GenerateToken(user);
        
        // Generate a new refresh token
        var newRefreshToken = await CreateRefreshTokenAsync(user);
        
        var response = new LoginResponse(accessToken, newRefreshToken.Token);
        
        await _refreshTokenRepository.SaveChangesAsync();
        
        return Result<LoginResponse>.Success(response);
    }
    
    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        return await _refreshTokenRepository.GetByTokenAsync(token);
    }

    public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
    {
        _refreshTokenRepository.Update(refreshToken);
        await _refreshTokenRepository.SaveChangesAsync();
    }
}