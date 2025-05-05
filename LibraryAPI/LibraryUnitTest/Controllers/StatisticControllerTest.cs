using LibraryAPI.Controllers;
using LibraryAPI.DTOs.StatisticDto;
using LibraryAPI.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LibraryUnitTest.Controllers
{
    class StatisticControllerTest
    {
        private Mock<IStatisticsService> _mockStatisticsService;
        private StatisticsController _controller;
        private StatisticsResponse _statistics;

        [SetUp]
        public void Setup()
        {
            _mockStatisticsService = new Mock<IStatisticsService>();
            _controller = new StatisticsController(_mockStatisticsService.Object);

            // Setup test statistics
            _statistics = new StatisticsResponse
            {
                TotalBooks = 50,
                TotalCategories = 10,
                TotalUsers = 25
            };
        }

        [Test]
        public async Task GetStatistics_ShouldReturnOk_WithStatistics()
        {
            // Arrange
            _mockStatisticsService.Setup(service => service.GetStatisticsAsync())
                .ReturnsAsync(_statistics);

            // Act
            var result = await _controller.GetStatistics();

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());

            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult!.Value, Is.EqualTo(_statistics));
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));

            var statisticsResult = okResult.Value as StatisticsResponse;
            Assert.That(statisticsResult, Is.Not.Null);
            Assert.That(statisticsResult!.TotalBooks, Is.EqualTo(_statistics.TotalBooks));
            Assert.That(statisticsResult.TotalCategories, Is.EqualTo(_statistics.TotalCategories));
            Assert.That(statisticsResult.TotalUsers, Is.EqualTo(_statistics.TotalUsers));
        }

        [Test]
        public async Task GetStatistics_ShouldVerifyServiceIsCalled()
        {
            // Arrange
            _mockStatisticsService.Setup(service => service.GetStatisticsAsync())
                .ReturnsAsync(_statistics);

            // Act
            await _controller.GetStatistics();

            // Assert
            _mockStatisticsService.Verify(service => service.GetStatisticsAsync(), Times.Once);
        }
    }
}
