using LibraryAPI.DTOs.StatisticDto;

namespace LibraryAPI.IRepository;

public interface IStatisticsRepository
{
    Task<StatisticsDto> GetStatisticsAsync();
}