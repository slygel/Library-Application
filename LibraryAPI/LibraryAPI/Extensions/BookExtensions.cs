using LibraryAPI.DTOs.BookDto;
using LibraryAPI.Entities;

namespace LibraryAPI.Extensions;

public static class BookExtensions
{
    public static BookResponse ToBookResponse(this Book book)
    {
        return new BookResponse
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            PublishDate = book.PublishDate,
            Isbn = book.Isbn,
            Quantity = book.Quantity,
            AvailableQuantity = book.AvailableQuantity,
            Description = book.Description,
            CategoryId = book.CategoryId
        };
    }
    
    public static Book ToBookAdd(this BookRequest request)
    {
        return new Book
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Author = request.Author,
            PublishDate = request.PublishDate,
            Isbn = request.Isbn,
            Quantity = request.Quantity,
            AvailableQuantity = request.AvailableQuantity,
            Description = request.Description,
            CategoryId = request.CategoryId
        };
    }
    
    // public static Book ToBookUpdate(this BookRequest request, Guid bookId)
    // {
    //     return new Book
    //     {
    //         Id = bookId,
    //         Title = request.Title,
    //         Author = request.Author,
    //         PublishDate = request.PublishDate,
    //         Isbn = request.Isbn,
    //         Quantity = request.Quantity,
    //         AvailableQuantity = request.AvailableQuantity,
    //         Description = request.Description,
    //         CategoryId = request.CategoryId
    //     };
    // }
    
    public static Book ToBookUpdate(this BookRequest request, Book existingBook)
    {
        existingBook.Title = request.Title;
        existingBook.Author = request.Author;
        existingBook.PublishDate = request.PublishDate;
        existingBook.Isbn = request.Isbn;
        existingBook.Quantity = request.Quantity;
        existingBook.AvailableQuantity = request.AvailableQuantity;
        existingBook.Description = request.Description;
        existingBook.CategoryId = request.CategoryId;
        return existingBook;
    }
}