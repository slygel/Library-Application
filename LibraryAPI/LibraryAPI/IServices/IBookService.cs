using LibraryAPI.DTOs.BookDto;
using LibraryAPI.Exceptions;
using LibraryAPI.Helpers;

namespace LibraryAPI.IServices;

public interface IBookService
{
    Task<PaginatedList<BookResponse>> GetAllAsync(int pageIndex, int pageSize, string? bookTitle = null, Guid? categoryId = null);
    Task<Result<BookResponse?>> GetByIdAsync(Guid id);
    Task<Result<BookResponse>> CreateAsync(BookRequest request);
    Task<Result<BookResponse>> UpdateAsync(Guid id, BookRequest request);
    Task<Result> DeleteByIdAsync(Guid id);
}