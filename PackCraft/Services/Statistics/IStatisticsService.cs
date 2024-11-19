using PackCraft.Models;

namespace PackCraft.Services.Statistics;

public interface IStatisticsService
{
    Task RecordStatisticsAsync(ProcessingStatistics statistics);
    Task SaveResultsAsync(string? customPath = null);
    IReadOnlyList<ProcessingStatistics> GetAllStatistics();
}