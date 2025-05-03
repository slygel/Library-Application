using LibraryAPI.Enums;

namespace LibraryAPI.Entities;

public class BookBorrowingRequest
{
    public Guid Id { get; set; }
    public DateTime RequestDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public Status Status { get; set; } // 1 = Approved; 2 = Rejected; 3 = Waiting
    
    public Guid ApproverId { get; set; }
    public User? Approver { get; set; }
    
    public Guid RequestorId { get; set; }
    public User? Requestor { get; set; }
    
    public ICollection<BookBorrowingRequestDetails> BookBorrowingRequestDetails { get; set; }
}