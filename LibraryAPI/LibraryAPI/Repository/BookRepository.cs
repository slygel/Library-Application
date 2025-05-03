using LibraryAPI.DbContext;
using LibraryAPI.Entities;
using LibraryAPI.IRepository;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Repository;

public class BookRepository : IBookRepository
{
    private readonly AppDbContext _context;

    public BookRepository(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<Book> GetAll(string? bookTitle = null, Guid? categoryId = null)
    {
        var query = _context.Books.AsNoTracking();
        
        if (!string.IsNullOrWhiteSpace(bookTitle))
        {
            query = query.Where(b => b.Title.Contains(bookTitle));
        }
        
        if (categoryId.HasValue)
        {
            query = query.Where(b => b.CategoryId == categoryId.Value);
        }
        
        return query;
    }

    public async Task<Book?> GetByIdAsync(Guid id)
    {
        return await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Book> CreateAsync(Book book)
    {
        await _context.Books.AddAsync(book);
        return book;
    }

    public Book Update(Book book)
    {
        _context.Books.Update(book);
        return book;
    }

    public void Delete(Book book)
    {
        _context.Books.Remove(book);
    }

    public async Task<ICollection<Book>> GetBooksByCategoryAsync(Guid categoryId)
    {
        var books = await _context.Books.AsNoTracking().Where(b => b.CategoryId == categoryId).ToListAsync();
        return books;
    }
    
    public async Task SaveChangesAsync(){
        await _context.SaveChangesAsync();
    }
}