using LibraryAPI.DTOs.CategoryDto;
using LibraryAPI.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAPI.Controllers;

[Route("/api/v1/categories")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllAsync(int pageIndex = 1, int pageSize = 10)
    {
        var categories = await _categoryService.GetAllAsync(pageIndex, pageSize);
        return Ok(categories);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryByIdAsync(Guid id)
    {
        var result = await _categoryService.GetByIdAsync(id);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode ?? StatusCodes.Status500InternalServerError, new { Error = result.ErrorMessage });
    }
    
    [HttpPost]
    [Authorize(policy:"Admin")]
    public async Task<IActionResult> CreateAsync([FromBody]CategoryRequest request)
    {
        var result = await _categoryService.CreateAsync(request);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode ?? StatusCodes.Status500InternalServerError, new { Error = result.ErrorMessage });
    }

    [HttpPut("{id}")]
    [Authorize(policy:"Admin")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody]CategoryRequest request)
    {
        var result = await _categoryService.UpdateAsync(id,request);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode ?? StatusCodes.Status500InternalServerError, new { Error = result.ErrorMessage });
    }
    
    [HttpDelete("{id}")]
    [Authorize(policy:"Admin")]
    public async Task<IActionResult> DeleteByIdAsync(Guid id)
    {
        var result = await _categoryService.DeleteByIdAsync(id);
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode ?? StatusCodes.Status500InternalServerError, new { Error = result.ErrorMessage });
    }
}