using Moq;
using LibraryAPI.DbContext;
using Microsoft.EntityFrameworkCore;
using LibraryAPI.Repository;
using LibraryAPI.Entities;

namespace LibraryUnitTest.Repository
{
    class CategoryRepositoryTest
    {
        private Mock<AppDbContext> _mockContext;
        private Mock<DbSet<Category>> _mockSet;
        private CategoryRepository _repository;
        private List<Category> _categories;

    }
}
