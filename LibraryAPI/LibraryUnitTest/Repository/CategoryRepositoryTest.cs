using LibraryAPI.DbContext;
using Microsoft.EntityFrameworkCore;
using LibraryAPI.Repository;
using LibraryAPI.Entities;

namespace LibraryUnitTest.Repository
{
    class CategoryRepositoryTest
    {
        private AppDbContext _context;
        private CategoryRepository _repository;
        private List<Category> _categories;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "LibraryDb_" + Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _repository = new CategoryRepository(_context);

            _categories = new List<Category>
               {
                   new Category { Id = Guid.NewGuid(), Name = "Fiction" },
                   new Category { Id = Guid.NewGuid(), Name = "Non-Fiction" }
               };

            _context.Categories.AddRange(_categories);
            _context.SaveChanges();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public void GetAll_ShouldReturnAllCategories()
        {
            // Act  
            var result = _repository.GetAll().ToList();

            // Assert  
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.Select(c => c.Name), Does.Contain("Fiction"));
            Assert.That(result.Select(c => c.Name), Does.Contain("Non-Fiction"));
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnCategory_WhenCategoryExists()
        {
            // Arrange
            var existingCategory = _categories[0];

            // Act
            var result = await _repository.GetByIdAsync(existingCategory.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(existingCategory.Id));
            Assert.That(result.Name, Is.EqualTo(existingCategory.Name));
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
        {
            // Act
            var result = await _repository.GetByIdAsync(Guid.NewGuid());

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task CreateAsync_ShouldAddNewCategory()
        {
            // Arrange
            var newCategory = new Category { Id = Guid.NewGuid(), Name = "Science" };

            // Act
            var result = await _repository.CreateAsync(newCategory);
            await _repository.SaveChangesAsync();

            // Assert
            var savedCategory = await _context.Categories.FindAsync(newCategory.Id);
            Assert.That(savedCategory, Is.Not.Null);
            Assert.That(savedCategory.Name, Is.EqualTo("Science"));
        }

        [Test]
        public async Task Update_ShouldUpdateExistingCategory()
        {
            // Arrange
            var categoryToUpdate = _categories[0];
            categoryToUpdate.Name = "Updated Category";

            // Act
            var result = _repository.Update(categoryToUpdate);
            await _repository.SaveChangesAsync();

            // Assert
            var updatedCategory = await _context.Categories.FindAsync(categoryToUpdate.Id);
            Assert.That(updatedCategory.Name, Is.EqualTo("Updated Category"));
        }

        [Test]
        public async Task Delete_ShouldRemoveCategory()
        {
            // Arrange
            var categoryToDelete = _categories[0];

            // Act
            _repository.Delete(categoryToDelete);
            await _repository.SaveChangesAsync();

            // Assert
            var deletedCategory = await _context.Categories.FindAsync(categoryToDelete.Id);
            Assert.That(deletedCategory, Is.Null);
        }

        [Test]
        public async Task SaveChangesAsync_ShouldPersistChanges()
        {
            // Arrange
            var newCategory = new Category { Id = Guid.NewGuid(), Name = "Test Category" };
            await _repository.CreateAsync(newCategory);

            // Act
            await _repository.SaveChangesAsync();

            // Assert
            var savedCategory = await _context.Categories.FindAsync(newCategory.Id);
            Assert.That(savedCategory, Is.Not.Null);
        }
    }
}
