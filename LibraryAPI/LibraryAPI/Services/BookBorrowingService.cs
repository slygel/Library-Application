using LibraryAPI.DTOs.BorrowingDto;
using LibraryAPI.Entities;
using LibraryAPI.Enums;
using LibraryAPI.Exceptions;
using LibraryAPI.Extensions;
using LibraryAPI.Helpers;
using LibraryAPI.IRepository;
using LibraryAPI.IServices;

namespace LibraryAPI.Services;

public class BookBorrowingService : IBookBorrowingService
{
    private readonly IBookBorrowingRepository _bookBorrowingRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IUserRepository _userRepository;

    public BookBorrowingService(
        IBookBorrowingRepository bookBorrowingRepository,
        IBookRepository bookRepository,
        IUserRepository userRepository)
    {
        _bookBorrowingRepository = bookBorrowingRepository;
        _bookRepository = bookRepository;
        _userRepository = userRepository;
    }

    public async Task<PaginatedList<BorrowingResponse>> GetAllBorrowingRequestsAsync(int pageIndex, int pageSize)
    {
        var requests = _bookBorrowingRepository.GetAll();
        var requestResponse = requests.Select(request => request.ToBorrowingResponse());
        return await PaginatedList<BorrowingResponse>.CreateAsync(requestResponse, pageIndex, pageSize);
    }
    
    public async Task<Result<PaginatedList<BorrowingResponse>>> GetUserBorrowingRequestsAsync(Guid userId, int pageIndex, int pageSize)
    {
        // Validate user exists
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return Result<PaginatedList<BorrowingResponse>>.Failure("User not found", StatusCodes.Status400BadRequest);
        }
    
        var requests = _bookBorrowingRepository.GetByUserId(userId);
        var responseList = requests.Select(request => request.ToBorrowingResponse());
        var paginatedList = await PaginatedList<BorrowingResponse>.CreateAsync(responseList, pageIndex, pageSize);
        return Result<PaginatedList<BorrowingResponse>>.Success(paginatedList);
    }

    public async Task<Result<RequestBorrowingResponse>> CreateBorrowingRequestAsync(BorrowingRequest requestDto, Guid userId)
    {
        // Validate user exists
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return Result<RequestBorrowingResponse>.Failure("User not found",StatusCodes.Status400BadRequest);
        }
        
        // Check if user has reached the monthly limit (3 borrowing requests per month)
        var monthlySummary = await _bookBorrowingRepository.GetCountForUserInCurrentMonthAsync(user.Id);
        if (monthlySummary >= 3)
        {
            return Result<RequestBorrowingResponse>.Failure("You have reached the maximum limit of 3 borrowing requests per month",StatusCodes.Status400BadRequest);
        }
    
        // Validate book count (max 5 books per request)
        if (requestDto.BookIds.Count > 5)
        {
            return Result<RequestBorrowingResponse>.Failure("You can borrow a maximum of 5 books per request",StatusCodes.Status400BadRequest);
        }
    
        if (requestDto.BookIds.Count == 0)
        {
            return Result<RequestBorrowingResponse>.Failure("You must specify at least one book to borrow",StatusCodes.Status400BadRequest);
        }
    
        // Validate all books exist and have available quantity
        foreach (var bookId in requestDto.BookIds)
        {
            var book = await _bookRepository.GetByIdAsync(bookId);
            if (book == null)
            {
                return Result<RequestBorrowingResponse>.Failure($"Book with ID {bookId} not found",StatusCodes.Status400BadRequest);
            }
            
            // Check if the book has available quantity
            if (book.AvailableQuantity <= 0)
            {
                return Result<RequestBorrowingResponse>.Failure($"Book with ID {bookId} is not available for borrowing",StatusCodes.Status400BadRequest);
            }
            
            book.AvailableQuantity -= 1;
            _bookRepository.Update(book);
        }
    
        // Get admin account
        var adminAccount = await _userRepository.GetByUsernameAsync("admin");
        if (adminAccount == null)
        {
            return Result<RequestBorrowingResponse>.Failure("Admin account not found",StatusCodes.Status400BadRequest);
        }
        // Create the borrowing request
        var bookBorrowingRequest = new BookBorrowingRequest
        {
            Id = Guid.NewGuid(),
            RequestDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddDays(30), // 30-day borrowing period
            Status = Status.Waiting,
            ApproverId = adminAccount.Id,
            RequestorId = user.Id,
            BookBorrowingRequestDetails = new List<BookBorrowingRequestDetails>()
        };
        
        // Add book details and decrease available quantity for each book
        foreach (var bookId in requestDto.BookIds)
        {
            bookBorrowingRequest.BookBorrowingRequestDetails.Add(new BookBorrowingRequestDetails
            {
                Id = Guid.NewGuid(),
                BookBorrowingRequestId = bookBorrowingRequest.Id,
                BookId = bookId
            });
        }
        
        // Save the request
        var createdRequest = await _bookBorrowingRepository.CreateAsync(bookBorrowingRequest);
        
        var response = createdRequest.ToBookBorrowingResponse();
        
        await _bookRepository.SaveChangesAsync();
        await _bookBorrowingRepository.SaveChangesAsync();
        return Result<RequestBorrowingResponse>.Success(response);
    }

    public async Task<Result<RequestBorrowingResponse>> UpdateBorrowingRequestStatusAsync(Guid borrowingRequestId, Status status, Guid userId)
    {
        // Get the borrowing request
        var borrowingRequest = await _bookBorrowingRepository.GetByIdAsync(borrowingRequestId);
        if (borrowingRequest == null)
        {
            return Result<RequestBorrowingResponse>.Failure($"Borrowing request with ID {borrowingRequestId} not found", StatusCodes.Status400BadRequest);
        }
        
        // Check if the request is in a valid state for status update
        if (borrowingRequest.Status != Status.Waiting)
        {
            return Result<RequestBorrowingResponse>.Failure(
                $"Cannot update status. The borrowing request is already {borrowingRequest.Status}.",
                StatusCodes.Status400BadRequest);
        }
    
        // Update the request status
        borrowingRequest.Status = status;
        borrowingRequest.ApproverId = userId;
    
        if (status == Status.Rejected)
        {
            // If the request is rejected, increase the available quantity of the books
            foreach (var detail in borrowingRequest.BookBorrowingRequestDetails)
            {
                var book = await _bookRepository.GetByIdAsync(detail.BookId);
                if(book == null){
                    return Result<RequestBorrowingResponse>.Failure($"Book with ID {detail.BookId} not found", StatusCodes.Status400BadRequest);
                }
                book.AvailableQuantity += 1;
                _bookRepository.Update(book);
            }
        }
        
        var updatedRequest = _bookBorrowingRepository.UpdateStatus(borrowingRequest);
        var response = updatedRequest.ToBookBorrowingResponse();

        await _bookRepository.SaveChangesAsync();
        await _bookBorrowingRepository.SaveChangesAsync();
        return Result<RequestBorrowingResponse>.Success(response);
    }
    
    public async Task<Result<int>> GetBorrowingRequestCountForUserInCurrentMonthAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return Result<int>.Failure("User not found",StatusCodes.Status400BadRequest);
        }
        
        var monthlySummary = await _bookBorrowingRepository.GetCountForUserInCurrentMonthAsync(user.Id);
        return Result<int>.Success(monthlySummary);
    }
} 