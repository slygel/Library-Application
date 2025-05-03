namespace LibraryAPI.DTOs.BorrowingDto;

public class BorrowingDetailResponse
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public string? BookTitle { get; set; }
} 