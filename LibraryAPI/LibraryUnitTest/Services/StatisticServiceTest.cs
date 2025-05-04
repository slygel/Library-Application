using LibraryAPI.DTOs.StatisticDto;
using LibraryAPI.IRepository;
using LibraryAPI.Services;
using Moq;

namespace LibraryUnitTest.Services
{
    class StatisticServiceTest
    {
        private Mock<IStatisticsRepository> _mockStatisticsRepository;
        private StatisticsService _statisticsService;
        private StatisticsDto _testStatistics;

        [SetUp]
        public void Setup()
        {
            _mockStatisticsRepository = new Mock<IStatisticsRepository>();
            _statisticsService = new StatisticsService(_mockStatisticsRepository.Object);

            // Setup test statistics
            _testStatistics = new StatisticsDto
            {
                TotalBooks = 100,
                TotalCategories = 10,
                TotalUsers = 25
            };
        }

        [Test]
        public async Task GetStatisticsAsync_ShouldReturnStatistics_FromRepository()
        {
            // Arrange
            _mockStatisticsRepository.Setup(repo => repo.GetStatisticsAsync())
                .ReturnsAsync(_testStatistics);

            // Act
            var result = await _statisticsService.GetStatisticsAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.TotalBooks, Is.EqualTo(_testStatistics.TotalBooks));
            Assert.That(result.TotalCategories, Is.EqualTo(_testStatistics.TotalCategories));
            Assert.That(result.TotalUsers, Is.EqualTo(_testStatistics.TotalUsers));

            // Verify repository method was called
            _mockStatisticsRepository.Verify(repo => repo.GetStatisticsAsync(), Times.Once);
        }

        [Test]
        public async Task GetStatisticsAsync_ShouldReturnEmptyStatistics_WhenRepositoryReturnsEmpty()
        {
            // Arrange
            var emptyStatistics = new StatisticsDto
            {
                TotalBooks = 0,
                TotalCategories = 0,
                TotalUsers = 0
            };

            _mockStatisticsRepository.Setup(repo => repo.GetStatisticsAsync())
                .ReturnsAsync(emptyStatistics);

            // Act
            var result = await _statisticsService.GetStatisticsAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.TotalBooks, Is.EqualTo(0));
            Assert.That(result.TotalCategories, Is.EqualTo(0));
            Assert.That(result.TotalUsers, Is.EqualTo(0));

            // Verify repository method was called
            _mockStatisticsRepository.Verify(repo => repo.GetStatisticsAsync(), Times.Once);
        }

        [Test]
        public async Task GetStatisticsAsync_ShouldPropagateException_WhenRepositoryThrowsException()
        {
            // Arrange
            _mockStatisticsRepository.Setup(repo => repo.GetStatisticsAsync())
                .ThrowsAsync(new Exception("Test exception"));

            // Act & Assert
            var exception = Assert.ThrowsAsync<Exception>(async () =>
                await _statisticsService.GetStatisticsAsync());

            Assert.That(exception.Message, Is.EqualTo("Test exception"));

            // Verify repository method was called
            _mockStatisticsRepository.Verify(repo => repo.GetStatisticsAsync(), Times.Once);
        }
    }
}
