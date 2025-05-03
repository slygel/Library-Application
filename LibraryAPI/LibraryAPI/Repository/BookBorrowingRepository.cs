using LibraryAPI.DbContext;
using LibraryAPI.Entities;
using LibraryAPI.Enums;
using LibraryAPI.IRepository;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Repository;

public class BookBorrowingRepository : IBookBorrowingRepository
{
    private readonly AppDbContext _dbContext;

    public BookBorrowingRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<BookBorrowingRequest> GetAll()
    {
        return _dbContext.BookBorrowingRequests
            .Include(b => b.BookBorrowingRequestDetails)
            .ThenInclude(d => d.Book)
            .Include(b => b.Requestor)
            .Include(b => b.Approver);
    }

    public IQueryable<BookBorrowingRequest> GetByUserId(Guid userId)
    {
        return _dbContext.BookBorrowingRequests
            .Where(b => b.RequestorId == userId)
            .Include(b => b.BookBorrowingRequestDetails)
            .ThenInclude(d => d.Book)
            .Include(b => b.Requestor);
    }
    
    public async Task<BookBorrowingRequest?> GetByIdAsync(Guid id)
    {
        return await _dbContext.BookBorrowingRequests
            .Include(b => b.BookBorrowingRequestDetails)
            .ThenInclude(d => d.Book)
            .Include(b => b.Requestor)
            .Include(b => b.Approver)
            .FirstOrDefaultAsync(b => b.Id == id);
    }
    
    public async Task<int> GetCountForUserInCurrentMonthAsync(Guid userId)
    {
        var today = DateTime.UtcNow;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
    
        var records = await _dbContext.BookBorrowingRequests
                        .Where(d => d.Status != Status.Rejected)
                        .CountAsync(b => b.RequestorId == userId && 
                          b.RequestDate >= firstDayOfMonth && 
                          b.RequestDate <= lastDayOfMonth);
        return records;
    }
    
    public async Task<BookBorrowingRequest> CreateAsync(BookBorrowingRequest bookBorrowingRequest)
    {
        await _dbContext.BookBorrowingRequests.AddAsync(bookBorrowingRequest);
        return bookBorrowingRequest;
    }
    
    public BookBorrowingRequest UpdateStatus(BookBorrowingRequest bookBorrowingRequest)
    {
        _dbContext.BookBorrowingRequests.Update(bookBorrowingRequest);
        return bookBorrowingRequest;
    }
    
    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
} 