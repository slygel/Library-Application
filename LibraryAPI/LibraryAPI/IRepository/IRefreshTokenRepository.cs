using LibraryAPI.Entities;

namespace LibraryAPI.IRepository;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task CreateAsync(RefreshToken refreshToken);
    void Update(RefreshToken refreshToken);
    Task RevokeAllUserTokensAsync(Guid userId);
    Task SaveChangesAsync();
}