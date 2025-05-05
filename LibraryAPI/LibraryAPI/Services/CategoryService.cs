using LibraryAPI.DTOs.CategoryDto;
using LibraryAPI.Exceptions;
using LibraryAPI.Extensions;
using LibraryAPI.Helpers;
using LibraryAPI.IRepository;
using LibraryAPI.IServices;

namespace LibraryAPI.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBookRepository _bookRepository;

    public CategoryService(ICategoryRepository categoryRepository, IBookRepository bookRepository)
    {
        _categoryRepository = categoryRepository;
        _bookRepository = bookRepository;
    }
    
    public async Task<PaginatedList<CategoryResponse>> GetAllAsync(int pageIndex, int pageSize)
    {
        var categories = _categoryRepository.GetAll();
        var categoryResponse = categories.Select(category => category.ToCategoryResponse());
        return await PaginatedList<CategoryResponse>.CreateAsync(categoryResponse, pageIndex, pageSize);
    }

    public async Task<Result<CategoryResponse?>> GetByIdAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return Result<CategoryResponse?>.Failure("Category not found", StatusCodes.Status404NotFound);
        }
        var categoryResponse = category.ToCategoryResponse();
        return Result<CategoryResponse?>.Success(categoryResponse);
    }
    
    public async Task<Result<CategoryResponse>> CreateAsync(CategoryRequest request)
    {
        var category = request.ToCategoryAdd();
        var result = await _categoryRepository.CreateAsync(category);

        var categoryResponse = result.ToCategoryResponse();
        
        await _categoryRepository.SaveChangesAsync();
        return Result<CategoryResponse>.Success(categoryResponse);
    }

    public async Task<Result<CategoryResponse>> UpdateAsync(Guid id, CategoryRequest request)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return Result<CategoryResponse>.Failure("Category not found", StatusCodes.Status404NotFound);
        }
        
        var categoryUpdate = request.ToCategoryUpdate(category);
        var result = _categoryRepository.Update(categoryUpdate);
        
        var categoryResponse = result.ToCategoryResponse();

        await _categoryRepository.SaveChangesAsync();
        return Result<CategoryResponse>.Success(categoryResponse);
    }
    public async Task<Result> DeleteByIdAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return Result.Failure("Category not found", StatusCodes.Status404NotFound);
        }
        
        var books = await _bookRepository.GetBooksByCategoryAsync(id);
        if (books.Count > 0)
        {
            return Result.Failure("Cannot delete category with books", StatusCodes.Status400BadRequest);
        }
        _categoryRepository.Delete(category);
        
        await _categoryRepository.SaveChangesAsync();
        return Result.Success();
    }
}