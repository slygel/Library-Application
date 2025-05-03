using LibraryAPI.Enums;

namespace LibraryAPI.DTOs.BorrowingDto;

public class RequestBorrowingResponse
{
    public Guid Id { get; set; }
    public DateTime RequestDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public Status Status { get; set; }
    public List<BorrowingDetailResponse> Books { get; set; }
}