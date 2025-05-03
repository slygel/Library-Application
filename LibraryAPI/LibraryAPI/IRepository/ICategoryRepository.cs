using LibraryAPI.Entities;

namespace LibraryAPI.IRepository;

public interface ICategoryRepository
{
    IQueryable<Category> GetAll();
    Task<Category?> GetByIdAsync(Guid id);
    Task<Category> CreateAsync(Category category);
    Category Update(Category category);
    void Delete(Category category);
    Task SaveChangesAsync();
}