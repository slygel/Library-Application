using LibraryAPI.Enums;

namespace LibraryAPI.Entities;

public class User
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public Role Role { get; set; }
    
    public ICollection<BookBorrowingRequest> RequestorBookBorrowingRequests { get; set; }
    public ICollection<BookBorrowingRequest> ApproverBookBorrowingRequests { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; }
}