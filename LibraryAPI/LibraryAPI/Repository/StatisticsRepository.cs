using LibraryAPI.DbContext;
using LibraryAPI.DTOs.StatisticDto;
using LibraryAPI.IRepository;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Repository;

public class StatisticsRepository : IStatisticsRepository
{
    private readonly AppDbContext _context;

    public StatisticsRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<StatisticsDto> GetStatisticsAsync()
    {
        var totalBooks = await _context.Books.CountAsync();
        var totalCategories = await _context.Categories.CountAsync();
        var totalUsers = await _context.Users.CountAsync() - 1;

        return new StatisticsDto
        {
            TotalBooks = totalBooks,
            TotalCategories = totalCategories,
            TotalUsers = totalUsers
        };
    }
}