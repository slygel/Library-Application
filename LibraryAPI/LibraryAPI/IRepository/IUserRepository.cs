using LibraryAPI.Entities;

namespace LibraryAPI.IRepository;

public interface IUserRepository
{
    Task CreateAsync(User user);
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task SaveChangesAsync();
}