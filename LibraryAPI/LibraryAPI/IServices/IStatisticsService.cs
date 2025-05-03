using LibraryAPI.DTOs.StatisticDto;

namespace LibraryAPI.IServices;

public interface IStatisticsService
{
    Task<StatisticsDto> GetStatisticsAsync();
}