namespace LibraryAPI.DTOs.CategoryDto;

public class CategoryResponse
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}