using LibraryAPI.DTOs.CategoryDto;
using LibraryAPI.Exceptions;
using LibraryAPI.Helpers;

namespace LibraryAPI.IServices;

public interface ICategoryService
{
    Task<PaginatedList<CategoryResponse>> GetAllAsync(int pageIndex, int pageSize);
    Task<Result<CategoryResponse?>> GetByIdAsync(Guid id);
    Task<Result<CategoryResponse>> CreateAsync(CategoryRequest request);
    Task<Result<CategoryResponse?>> UpdateAsync(Guid id, CategoryRequest request);
    Task<Result> DeleteByIdAsync(Guid id);
}