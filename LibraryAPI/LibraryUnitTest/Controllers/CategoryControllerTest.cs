using LibraryAPI.Controllers;
using LibraryAPI.DTOs.CategoryDto;
using LibraryAPI.Exceptions;
using LibraryAPI.Helpers;
using LibraryAPI.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LibraryUnitTest.Controllers
{
    class CategoryControllerTest
    {
        private Mock<ICategoryService> _mockCategoryService;
        private CategoryController _controller;
        private CategoryResponse _testCategory;
        private List<CategoryResponse> _testCategories;

        [SetUp]
        public void Setup()
        {
            _mockCategoryService = new Mock<ICategoryService>();
            _controller = new CategoryController(_mockCategoryService.Object);

            // Setup test category
            _testCategory = new CategoryResponse
            {
                Id = Guid.NewGuid(),
                Name = "Test Category",
                Description = "Test Category Description"
            };

            // Setup test categories list
            _testCategories = new List<CategoryResponse>
            {
                _testCategory,
                new CategoryResponse
                {
                    Id = Guid.NewGuid(),
                    Name = "Fiction",
                    Description = "Fiction books"
                },
                new CategoryResponse
                {
                    Id = Guid.NewGuid(),
                    Name = "Non-Fiction",
                    Description = "Non-fiction books"
                }
            };
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnOk_WithPaginatedCategories()
        {
            // Arrange
            var paginatedList = new PaginatedList<CategoryResponse>(_testCategories, 3, 1, 1);
            _mockCategoryService.Setup(service => service.GetAllAsync(1, 10))
                .ReturnsAsync(paginatedList);

            // Act
            var result = await _controller.GetAllAsync(1, 10);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());

            var okResult = result as OkObjectResult;
            Assert.That(okResult!.Value, Is.EqualTo(paginatedList));
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }


        [Test]
        public async Task GetCategoryByIdAsync_ShouldReturnOk_WhenCategoryExists()
        {
            // Arrange
            var categoryId = _testCategory.Id;
            _mockCategoryService.Setup(service => service.GetByIdAsync(categoryId))
                .ReturnsAsync(Result<CategoryResponse>.Success(_testCategory));

            // Act
            var result = await _controller.GetCategoryByIdAsync(categoryId);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());

            var okResult = result as OkObjectResult;
            Assert.That(okResult!.Value, Is.EqualTo(_testCategory));
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }

        [Test]
        public async Task GetCategoryByIdAsync_ShouldReturnNotFound_WhenCategoryDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            _mockCategoryService.Setup(service => service.GetByIdAsync(nonExistentId))
                .ReturnsAsync(Result<CategoryResponse>.Failure("Category not found", StatusCodes.Status404NotFound));

            // Act
            var result = await _controller.GetCategoryByIdAsync(nonExistentId);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());

            var objectResult = result as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        }


        [Test]
        public async Task CreateAsync_ShouldReturnOk_WhenRequestIsValid()
        {
            // Arrange
            var categoryRequest = new CategoryRequest
            {
                Name = "New Category",
                Description = "New Category Description"
            };

            _mockCategoryService.Setup(service => service.CreateAsync(categoryRequest))
                .ReturnsAsync(Result<CategoryResponse>.Success(_testCategory));

            // Act
            var result = await _controller.CreateAsync(categoryRequest);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());

            var okResult = result as OkObjectResult;
            Assert.That(okResult!.Value, Is.EqualTo(_testCategory));
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }

        [Test]
        public async Task CreateAsync_ShouldReturnBadRequest_WhenRequestIsInvalid()
        {
            // Arrange
            var invalidRequest = new CategoryRequest
            {
                Name = "", // Invalid - empty name
                Description = "Description"
            };

            _mockCategoryService.Setup(service => service.CreateAsync(invalidRequest))
                .ReturnsAsync(Result<CategoryResponse>.Failure("Name is required", StatusCodes.Status400BadRequest));

            // Act
            var result = await _controller.CreateAsync(invalidRequest);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());

            var objectResult = result as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }


        [Test]
        public async Task UpdateAsync_ShouldReturnOk_WhenRequestIsValid()
        {
            // Arrange
            var categoryId = _testCategory.Id;
            var updateRequest = new CategoryRequest
            {
                Name = "Updated Category",
                Description = "Updated Description"
            };

            var updatedCategory = new CategoryResponse
            {
                Id = categoryId,
                Name = updateRequest.Name,
                Description = updateRequest.Description
            };

            _mockCategoryService.Setup(service => service.UpdateAsync(categoryId, updateRequest))
                .ReturnsAsync(Result<CategoryResponse>.Success(updatedCategory));

            // Act
            var result = await _controller.UpdateAsync(categoryId, updateRequest);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());

            var okResult = result as OkObjectResult;
            Assert.That(okResult!.Value, Is.EqualTo(updatedCategory));
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }

        [Test]
        public async Task UpdateAsync_ShouldReturnNotFound_WhenCategoryDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var updateRequest = new CategoryRequest
            {
                Name = "Updated Category",
                Description = "Updated Description"
            };

            _mockCategoryService.Setup(service => service.UpdateAsync(nonExistentId, updateRequest))
                .ReturnsAsync(Result<CategoryResponse>.Failure("Category not found", StatusCodes.Status404NotFound));

            // Act
            var result = await _controller.UpdateAsync(nonExistentId, updateRequest);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());

            var objectResult = result as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        }
    }
}
