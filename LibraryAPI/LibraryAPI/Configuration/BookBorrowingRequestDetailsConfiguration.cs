using LibraryAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryAPI.Configuration;

public class BookBorrowingRequestDetailsConfiguration : IEntityTypeConfiguration<BookBorrowingRequestDetails>
{
    public void Configure(EntityTypeBuilder<BookBorrowingRequestDetails> builder)
    {
        builder.ToTable("BookBorrowingRequestDetails");
        builder.HasKey(b => b.Id);
        
        builder.HasOne(b => b.Book)
            .WithMany(b => b.BookBorrowingRequestDetails)
            .HasForeignKey(b => b.BookId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        
        builder.HasOne(b => b.BookBorrowingRequest)
            .WithMany(b => b.BookBorrowingRequestDetails)
            .HasForeignKey(b => b.BookBorrowingRequestId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}