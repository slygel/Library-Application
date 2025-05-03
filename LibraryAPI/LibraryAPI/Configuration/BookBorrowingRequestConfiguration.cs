using LibraryAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryAPI.Configuration;

public class BookBorrowingRequestConfiguration : IEntityTypeConfiguration<BookBorrowingRequest>
{
    public void Configure(EntityTypeBuilder<BookBorrowingRequest> builder)
    {
        builder.ToTable("BookBorrowingRequest");
        builder.HasKey(b => b.Id);
        
        builder.HasOne(u => u.Requestor)
            .WithMany(b => b.RequestorBookBorrowingRequests)
            .HasForeignKey(u => u.RequestorId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        
        builder.HasOne(u => u.Approver)
            .WithMany(b => b.ApproverBookBorrowingRequests)
            .HasForeignKey(u => u.ApproverId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}