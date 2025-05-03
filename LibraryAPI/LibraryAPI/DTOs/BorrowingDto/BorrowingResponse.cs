using LibraryAPI.DTOs.UserDto;
using LibraryAPI.Enums;

namespace LibraryAPI.DTOs.BorrowingDto;

public class BorrowingResponse
{
    public Guid Id { get; set; }
    public DateTime RequestDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public Status Status { get; set; }
    public UserResponse? Requestor { get; set; }
    public List<BorrowingDetailResponse> Books { get; set; }
}
