using LibraryAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryAPI.Configuration;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Category");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(200);

        builder.HasData(
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Science Fiction",
                Description = "Books that explore futuristic concepts and advanced technologies."
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Fantasy",
                Description = "Books that contain magical elements and fantastical worlds."
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Mystery",
                Description = "Books that involve solving a crime or uncovering secrets."
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Romance",
                Description = "Books that focus on romantic relationships."
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Non-Fiction",
                Description = "Books that are based on real events and facts."
            }
        );
    }
}