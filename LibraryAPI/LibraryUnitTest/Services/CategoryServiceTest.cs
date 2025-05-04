using LibraryAPI.DTOs.CategoryDto;
using LibraryAPI.Entities;
using LibraryAPI.IRepository;
using LibraryAPI.Services;
using Microsoft.AspNetCore.Http;
using Moq;

namespace LibraryUnitTest.Services
{
    class CategoryServiceTest
    {
        private Mock<ICategoryRepository> _mockCategoryRepository;
        private Mock<IBookRepository> _mockBookRepository;
        private CategoryService _categoryService;
        private List<Category> _categories;

        [SetUp]
        public void Setup()
        {
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockBookRepository = new Mock<IBookRepository>();
            _categoryService = new CategoryService(_mockCategoryRepository.Object, _mockBookRepository.Object);

            // Setup test data
            _categories = new List<Category>
            {
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Fiction",
                    Description = "Fiction books"
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Non-Fiction",
                    Description = "Non-fiction books"
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Science",
                    Description = "Science books"
                }
            };
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnCategory_WhenCategoryExists()
        {
            // Arrange
            var categoryId = _categories[0].Id;
            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync(_categories[0]);

            // Act
            var result = await _categoryService.GetByIdAsync(categoryId);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.Id, Is.EqualTo(categoryId));
            Assert.That(result.Value.Name, Is.EqualTo(_categories[0].Name));
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnFailure_WhenCategoryDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(nonExistentId))
                .ReturnsAsync((Category)null);

            // Act
            var result = await _categoryService.GetByIdAsync(nonExistentId);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Category not found"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        }

        [Test]
        public async Task CreateAsync_ShouldCreateCategory()
        {
            // Arrange
            var categoryRequest = new CategoryRequest
            {
                Name = "New Category",
                Description = "New Description"
            };

            _mockCategoryRepository.Setup(repo => repo.CreateAsync(It.IsAny<Category>()))
                .ReturnsAsync((Category c) => c);

            // Act
            var result = await _categoryService.CreateAsync(categoryRequest);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.Name, Is.EqualTo(categoryRequest.Name));
            Assert.That(result.Value.Description, Is.EqualTo(categoryRequest.Description));

            _mockCategoryRepository.Verify(repo => repo.CreateAsync(It.IsAny<Category>()), Times.Once);
            _mockCategoryRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task UpdateAsync_ShouldUpdateCategory_WhenCategoryExists()
        {
            // Arrange
            var categoryId = _categories[0].Id;
            var categoryRequest = new CategoryRequest
            {
                Name = "Updated Name",
                Description = "Updated Description"
            };

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync(_categories[0]);
            _mockCategoryRepository.Setup(repo => repo.Update(It.IsAny<Category>()))
                .Returns((Category c) => c);

            // Act
            var result = await _categoryService.UpdateAsync(categoryId, categoryRequest);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.Name, Is.EqualTo(categoryRequest.Name));
            Assert.That(result.Value.Description, Is.EqualTo(categoryRequest.Description));

            _mockCategoryRepository.Verify(repo => repo.Update(It.IsAny<Category>()), Times.Once);
            _mockCategoryRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task UpdateAsync_ShouldReturnFailure_WhenCategoryDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var categoryRequest = new CategoryRequest
            {
                Name = "Updated Name",
                Description = "Updated Description"
            };

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(nonExistentId))
                .ReturnsAsync((Category)null);

            // Act
            var result = await _categoryService.UpdateAsync(nonExistentId, categoryRequest);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Category not found"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));

            _mockCategoryRepository.Verify(repo => repo.Update(It.IsAny<Category>()), Times.Never);
            _mockCategoryRepository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public async Task DeleteByIdAsync_ShouldDeleteCategory_WhenCategoryExistsAndHasNoBooks()
        {
            // Arrange
            var categoryId = _categories[0].Id;
            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync(_categories[0]);
            _mockBookRepository.Setup(repo => repo.GetBooksByCategoryAsync(categoryId))
                .ReturnsAsync(new List<Book>());

            // Act
            var result = await _categoryService.DeleteByIdAsync(categoryId);

            // Assert
            Assert.That(result.IsSuccess, Is.True);

            _mockCategoryRepository.Verify(repo => repo.Delete(It.IsAny<Category>()), Times.Once);
            _mockCategoryRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task DeleteByIdAsync_ShouldReturnFailure_WhenCategoryDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(nonExistentId))
                .ReturnsAsync((Category)null);

            // Act
            var result = await _categoryService.DeleteByIdAsync(nonExistentId);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Category not found"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));

            _mockCategoryRepository.Verify(repo => repo.Delete(It.IsAny<Category>()), Times.Never);
            _mockCategoryRepository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public async Task DeleteByIdAsync_ShouldReturnFailure_WhenCategoryHasBooks()
        {
            // Arrange
            var categoryId = _categories[0].Id;
            var books = new List<Book>
            {
                new Book {
                    Id = Guid.NewGuid(),
                    Title = "Clean Code",
                    Author = "Robert C. Martin",
                    CategoryId = categoryId,
                    PublishDate = new DateOnly(1990, 10, 15),
                    Description = "A book about clean code"
                },
                new Book {
                    Id = Guid.NewGuid(),
                    Title = "Clean Code",
                    Author = "Robert C. Martin",
                    CategoryId = categoryId,
                    PublishDate = new DateOnly(1990, 10, 15),
                    Description = "A book about clean code"
                }
            };

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync(_categories[0]);
            _mockBookRepository.Setup(repo => repo.GetBooksByCategoryAsync(categoryId))
                .ReturnsAsync(books);

            // Act
            var result = await _categoryService.DeleteByIdAsync(categoryId);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Cannot delete category with books"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));

            _mockCategoryRepository.Verify(repo => repo.Delete(It.IsAny<Category>()), Times.Never);
            _mockCategoryRepository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }
    }
}
