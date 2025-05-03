using LibraryAPI.DTOs.BorrowingDto;
using LibraryAPI.DTOs.UserDto;
using LibraryAPI.Entities;

namespace LibraryAPI.Extensions;

public static class BorrowingExtensions
{
    public static List<BorrowingResponse> ToBorrowingResponses(this List<BookBorrowingRequest> requests)
    {
        return requests.Select(request => request.ToBorrowingResponse()).ToList();
    }
    
    public static BorrowingResponse ToBorrowingResponse(this BookBorrowingRequest request)
    {
        return new BorrowingResponse
        {
            Id = request.Id,
            RequestDate = request.RequestDate,
            ExpirationDate = request.ExpirationDate,
            Status = request.Status,
            Requestor = new UserResponse
            {
                Id = request.Requestor.Id,
                Name = request.Requestor.Name,
                Email = request.Requestor.Email,
                PhoneNumber = request.Requestor.PhoneNumber
            },
            Books = request.BookBorrowingRequestDetails.Select(detail => detail.ToBorrowingDetailResponse()).ToList() ?? new List<BorrowingDetailResponse>(),
        };
    }

    public static RequestBorrowingResponse ToBookBorrowingResponse(this BookBorrowingRequest request)
    {
        return new RequestBorrowingResponse
        {
            Id = request.Id,
            RequestDate = request.RequestDate,
            ExpirationDate = request.ExpirationDate,
            Status = request.Status,
            Books = request.BookBorrowingRequestDetails.Select(detail => detail.ToBorrowingDetailResponse()).ToList() ?? new List<BorrowingDetailResponse>(),
        };
    }
    
    private static BorrowingDetailResponse ToBorrowingDetailResponse(this BookBorrowingRequestDetails detail)
    {
        return new BorrowingDetailResponse
        {
            Id = detail.Id,
            BookId = detail.BookId,
            BookTitle = detail.Book?.Title
        };
    }
}