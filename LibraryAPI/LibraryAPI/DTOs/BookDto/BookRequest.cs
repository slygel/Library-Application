using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.DTOs.BookDto;

public class BookRequest
{
    [Required]
    public required string Title { get; set; }
    
    [Required]
    public required string Author { get; set; }
    
    [Required]
    public DateOnly PublishDate { get; set; }
    
    public string? Isbn { get; set; }
    
    [Required]
    public int Quantity { get; set; }
    
    [Required]
    public int AvailableQuantity { get; set; }
    
    public string? Description { get; set; }

    [Required]
    public Guid CategoryId { get; set; }
}