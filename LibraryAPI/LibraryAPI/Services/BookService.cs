using LibraryAPI.DTOs.BookDto;
using LibraryAPI.Exceptions;
using LibraryAPI.Extensions;
using LibraryAPI.Helpers;
using LibraryAPI.IRepository;
using LibraryAPI.IServices;

namespace LibraryAPI.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly ICategoryRepository _categoryRepository;

    public BookService(IBookRepository bookRepository, ICategoryRepository categoryRepository)
    {
        _bookRepository = bookRepository;
        _categoryRepository = categoryRepository;
    }
    
    public async Task<PaginatedList<BookResponse>> GetAllAsync(int pageIndex, int pageSize, string? bookTitle = null, Guid? categoryId = null)
    {
        var books = _bookRepository.GetAll(bookTitle, categoryId).OrderByDescending(b => b.PublishDate);
        var bookResponse = books.Select(book => book.ToBookResponse());
        return await PaginatedList<BookResponse>.CreateAsync(bookResponse, pageIndex, pageSize);
    }

    public async Task<Result<BookResponse?>> GetByIdAsync(Guid id)
    {
        var book = await _bookRepository.GetByIdAsync(id);
        if (book == null)
        {
            return Result<BookResponse?>.Failure("Book not found", StatusCodes.Status404NotFound);
        }
        var bookResponse = book.ToBookResponse();
        return Result<BookResponse?>.Success(bookResponse);
    }

    public async Task<Result<BookResponse>> CreateAsync(BookRequest request)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (category == null)
        {
            return Result<BookResponse>.Failure("Category not found", StatusCodes.Status404NotFound);
        }

        if (request.PublishDate > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return Result<BookResponse>.Failure("Publish date cannot be in the future", StatusCodes.Status400BadRequest);
        }

        if (request.AvailableQuantity > request.Quantity)
        {
            return Result<BookResponse>.Failure("Available can't bigger quantity", StatusCodes.Status400BadRequest);
        }

        if(request.AvailableQuantity != request.Quantity)
        {
            return Result<BookResponse>.Failure("Available must equal quantity", StatusCodes.Status400BadRequest);
        }
        
        var book = request.ToBookAdd();
        var result = await _bookRepository.CreateAsync(book);
        
        var bookResponse = result.ToBookResponse();
        
        await _bookRepository.SaveChangesAsync();
        return Result<BookResponse>.Success(bookResponse);
    }

    public async Task<Result<BookResponse>> UpdateAsync(Guid id, BookRequest request)
    {
        var book = await _bookRepository.GetByIdAsync(id);
        if (book == null)
        {
            return Result<BookResponse>.Failure("Book not found", StatusCodes.Status404NotFound);
        }

        if (request.PublishDate > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return Result<BookResponse>.Failure("Publish date cannot be in the future", StatusCodes.Status400BadRequest);
        }

        if (request.AvailableQuantity > request.Quantity)
        {
            return Result<BookResponse>.Failure("Available can't bigger quantity", StatusCodes.Status400BadRequest);
        }
        
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (category == null)
        {
            return Result<BookResponse>.Failure("Category not found", StatusCodes.Status404NotFound);
        }
        
        var updatedBook = request.ToBookUpdate(book);
        var result = _bookRepository.Update(updatedBook);
        
        var bookResponse = result.ToBookResponse();
        
        await _bookRepository.SaveChangesAsync();
        return Result<BookResponse>.Success(bookResponse);
    }

    public async Task<Result> DeleteByIdAsync(Guid id)
    {
        var book = await _bookRepository.GetByIdAsync(id);
        if (book == null)
        {
            return Result.Failure("Book not found", StatusCodes.Status404NotFound);
        }
        
        if(book.Quantity != book.AvailableQuantity)
        {
            return Result.Failure("Cannot delete book when borrowing", StatusCodes.Status400BadRequest);
        }
        
        _bookRepository.Delete(book);
        await _bookRepository.SaveChangesAsync();
        return Result.Success();
    }
}