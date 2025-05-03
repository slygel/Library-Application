using LibraryAPI.DTOs.CategoryDto;
using LibraryAPI.Entities;

namespace LibraryAPI.Extensions;

public static class CategoryExtensions
{
    public static CategoryResponse ToCategoryResponse(this Category category)
    {
        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }
    
    public static Category ToCategoryAdd(this CategoryRequest request)
    {
        return new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description ?? string.Empty,
            Books = new List<Book>()
        };
    }
    
    public static Category ToCategoryUpdate(this CategoryRequest request, Guid categoryId)
    {
        return new Category
        {
            Id = categoryId,
            Name = request.Name,
            Description = request.Description ?? string.Empty,
            Books = new List<Book>()
        };
    }
    
    public static Category ToCategoryUpdate(this CategoryRequest request, Category existingCategory)
    {
        existingCategory.Name = request.Name;
        existingCategory.Description = request.Description ?? string.Empty;
        return existingCategory;
    }
}