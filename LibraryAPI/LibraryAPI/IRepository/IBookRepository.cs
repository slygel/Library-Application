using LibraryAPI.Entities;

namespace LibraryAPI.IRepository;

public interface IBookRepository
{
    IQueryable<Book> GetAll(string? bookTitle = null, Guid? categoryId = null);
    Task<Book?> GetByIdAsync(Guid id);
    Task<Book> CreateAsync(Book book);
    Book Update(Book book);
    void Delete(Book book);
    Task<ICollection<Book>> GetBooksByCategoryAsync(Guid categoryId);
    Task SaveChangesAsync();
}