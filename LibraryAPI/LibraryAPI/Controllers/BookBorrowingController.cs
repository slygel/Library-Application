using System.Security.Claims;
using LibraryAPI.Enums;
using LibraryAPI.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LibraryAPI.DTOs.BorrowingDto;

namespace LibraryAPI.Controllers;

[ApiController]
[Route("api/v1/book-borrowing")]
public class BookBorrowingController : ControllerBase
{
    private readonly IBookBorrowingService _bookBorrowingService;

    public BookBorrowingController(IBookBorrowingService bookBorrowingService)
    {
        _bookBorrowingService = bookBorrowingService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllBorrowingRequests(int pageIndex = 1, int pageSize = 10)
    {
        var requests = await _bookBorrowingService.GetAllBorrowingRequestsAsync(pageIndex, pageSize);
        return Ok(requests);
    }
    
    [HttpGet("my-requests")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> GetMyBorrowingRequests(int pageIndex = 1, int pageSize = 10)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty);
        var result = await _bookBorrowingService.GetUserBorrowingRequestsAsync(userId, pageIndex, pageSize);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode ?? StatusCodes.Status500InternalServerError, new { Error = result.ErrorMessage });
    }
    
    [HttpPost]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> CreateBorrowingRequest(BorrowingRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty);
        var result = await _bookBorrowingService.CreateBorrowingRequestAsync(request, userId);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode ?? StatusCodes.Status500InternalServerError, new { Error = result.ErrorMessage });
    }
    
    [HttpPut("{id}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveBorrowingRequest(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty);
        var result = await _bookBorrowingService.UpdateBorrowingRequestStatusAsync(id, Status.Approved, userId);
        return result.IsSuccess 
            ? Ok(result.Value) 
            : StatusCode(result.StatusCode ?? StatusCodes.Status500InternalServerError, new { Error = result.ErrorMessage });
    }
    
    [HttpPut("{id}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RejectBorrowingRequest(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty);
        var result = await _bookBorrowingService.UpdateBorrowingRequestStatusAsync(id, Status.Rejected, userId);
        return result.IsSuccess 
            ? Ok(result.Value) 
            : StatusCode(result.StatusCode ?? StatusCodes.Status500InternalServerError, new { Error = result.ErrorMessage });
    }
    
    [HttpGet("monthly-count")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> GetMyBorrowingRequestCountForUser()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty);
        var result = await _bookBorrowingService.GetBorrowingRequestCountForUserInCurrentMonthAsync(userId);
        return result.IsSuccess 
            ? Ok(result.Value) 
            : StatusCode(result.StatusCode ?? StatusCodes.Status500InternalServerError, new { Error = result.ErrorMessage });
    }
} 