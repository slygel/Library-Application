using LibraryAPI.DTOs.BookDto;
using LibraryAPI.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAPI.Controllers;

[Route("/api/v1/books")]
[ApiController]
public class BookController : ControllerBase
{
    private readonly IBookService _bookService;

    public BookController(IBookService bookService)
    {
        _bookService = bookService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllAsync(
        int pageIndex = 1, 
        int pageSize = 10,
        [FromQuery] string? bookTitle = null,
        [FromQuery] Guid? categoryId = null)
    {
        var books = await _bookService.GetAllAsync(pageIndex, pageSize, bookTitle, categoryId);
        return Ok(books);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryByIdAsync(Guid id)
    {
        var result = await _bookService.GetByIdAsync(id);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode ?? StatusCodes.Status500InternalServerError, new { Error = result.ErrorMessage });
    }
    
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateAsync([FromBody]BookRequest request)
    {
        var result = await _bookService.CreateAsync(request);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode ?? StatusCodes.Status500InternalServerError, new { Error = result.ErrorMessage });
    }
    
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody]BookRequest request)
    {
        var result = await _bookService.UpdateAsync(id,request);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode ?? StatusCodes.Status500InternalServerError, new { Error = result.ErrorMessage });
    }
    
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteByIdAsync(Guid id)
    {
        var result = await _bookService.DeleteByIdAsync(id);
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode ?? StatusCodes.Status500InternalServerError, new { Error = result.ErrorMessage });
    }
}