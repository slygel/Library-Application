using LibraryAPI.DTOs.BorrowingDto;
using LibraryAPI.Enums;
using LibraryAPI.Exceptions;
using LibraryAPI.Helpers;

namespace LibraryAPI.IServices;

public interface IBookBorrowingService
{
    Task<PaginatedList<BorrowingResponse>> GetAllBorrowingRequestsAsync(int pageIndex, int pageSize, Status? status = null);
    Task<Result<PaginatedList<BorrowingResponse>>> GetUserBorrowingRequestsAsync(Guid userId, int pageIndex, int pageSize, Status? status = null);
    Task<Result<RequestBorrowingResponse>> CreateBorrowingRequestAsync(BorrowingRequest request, Guid userId);
    Task<Result<RequestBorrowingResponse>> UpdateBorrowingRequestStatusAsync(Guid id, Status status, Guid userId);
    Task<Result<int>> GetBorrowingRequestCountForUserInCurrentMonthAsync(Guid userId);
} 