using LibraryAPI.DbContext;
using LibraryAPI.Entities;
using LibraryAPI.IRepository;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Repository;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public IQueryable<Category> GetAll()
    {
        return _context.Categories.AsNoTracking();
    }

    public async Task<Category?> GetByIdAsync(Guid id)
    {
        return await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Category> CreateAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
        return category;
    }

    public Category Update(Category category)
    { 
        _context.Categories.Update(category);
        return category;
    }

    public void Delete(Category category)
    {
        _context.Categories.Remove(category);
    }
    
    public async Task SaveChangesAsync(){
        await _context.SaveChangesAsync();
    }
}