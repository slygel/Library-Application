using LibraryAPI.DTOs.StatisticDto;
using LibraryAPI.IRepository;
using LibraryAPI.IServices;

namespace LibraryAPI.Services;

public class StatisticsService : IStatisticsService
{
    private readonly IStatisticsRepository _statisticsRepository;

    public StatisticsService(IStatisticsRepository statisticsRepository)
    {
        _statisticsRepository = statisticsRepository;
    }

    public async Task<StatisticsDto> GetStatisticsAsync()
    {
        return await _statisticsRepository.GetStatisticsAsync();
    }
}