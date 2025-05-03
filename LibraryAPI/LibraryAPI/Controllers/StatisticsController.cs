using LibraryAPI.DTOs.StatisticDto;
using LibraryAPI.IServices;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAPI.Controllers;

[ApiController]
[Route("api/v1/statistics")]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticsService _statisticsService;

    public StatisticsController(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }
    
    [HttpGet]
    public async Task<ActionResult<StatisticsDto>> GetStatistics()
    {
        var statistics = await _statisticsService.GetStatisticsAsync();
        return Ok(statistics);
    }
}