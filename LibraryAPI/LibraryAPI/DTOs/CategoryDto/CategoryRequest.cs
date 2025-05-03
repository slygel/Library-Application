using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.DTOs.CategoryDto;

public class CategoryRequest
{
    [Required]
    public required string Name { get; set; }
    
    [Required]
    public required string Description { get; set; }
}