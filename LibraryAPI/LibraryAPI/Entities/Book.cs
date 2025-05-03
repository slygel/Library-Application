namespace LibraryAPI.Entities;

public class Book
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Author { get; set; }
    public DateOnly PublishDate { get; set; }
    public string? Isbn { get; set; }
    public int Quantity { get; set; }
    public int AvailableQuantity { get; set; }
    public string? Description { get; set; }
    
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }
    
    public ICollection<BookBorrowingRequestDetails> BookBorrowingRequestDetails { get; set; }
}