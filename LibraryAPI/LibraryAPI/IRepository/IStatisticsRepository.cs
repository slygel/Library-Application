using LibraryAPI.DTOs.StatisticDto;

namespace LibraryAPI.IRepository;

public interface IStatisticsRepository
{
    Task<StatisticsResponse> GetStatisticsAsync();
}