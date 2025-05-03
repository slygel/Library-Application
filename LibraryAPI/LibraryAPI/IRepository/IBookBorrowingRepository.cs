using LibraryAPI.Entities;

namespace LibraryAPI.IRepository;

public interface IBookBorrowingRepository
{
    IQueryable<BookBorrowingRequest> GetAll();
    IQueryable<BookBorrowingRequest> GetByUserId(Guid userId);
    Task<BookBorrowingRequest?> GetByIdAsync(Guid id);
    Task<int> GetCountForUserInCurrentMonthAsync(Guid userId);
    Task<BookBorrowingRequest> CreateAsync(BookBorrowingRequest bookBorrowingRequest);
    BookBorrowingRequest UpdateStatus(BookBorrowingRequest bookBorrowingRequest);
    Task SaveChangesAsync();
} 