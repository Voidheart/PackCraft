using System.Text;
using Microsoft.Extensions.Logging;
using PackCraft.Models;

namespace PackCraft.Services.Statistics;

public class StatisticsService(ILogger<StatisticsService> logger, ApplicationContext context)
    : IStatisticsService
{
    private readonly List<ProcessingStatistics> _statistics = new();

    public async Task RecordStatisticsAsync(ProcessingStatistics statistics)
    {
        _statistics.Add(statistics);
        logger.LogInformation(
            "Recorded statistics for {SpriteSheet}: {Width}x{Height}, {Total} units ({Processed} processed)",
            statistics.SpriteSheetName,
            statistics.Dimensions.Width,
            statistics.Dimensions.Height,
            statistics.TotalUnits,
            statistics.ProcessedUnits
        );
    }

    public IReadOnlyList<ProcessingStatistics> GetAllStatistics()
    {
        return _statistics.AsReadOnly();
    }

    public async Task SaveResultsAsync(string? customPath = null)
    {
        string? resultsPath = customPath ?? context.UnpackOptions.ResultsPath ?? "processing_results.txt";
        StringBuilder? content = new();

        content.AppendLine("Sprite Sheet Processing Results");
        content.AppendLine("==============================");
        content.AppendLine($"Processing Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        content.AppendLine();

        foreach (ProcessingStatistics? stat in _statistics)
        {
            content.AppendLine($"Sprite Sheet: {stat.SpriteSheetName}");
            content.AppendLine($"Dimensions: {stat.Dimensions.Width}x{stat.Dimensions.Height}");
            content.AppendLine($"Total Units: {stat.TotalUnits}");
            content.AppendLine($"Processed Units: {stat.ProcessedUnits}");
            content.AppendLine($"Processing Time: {stat.ProcessingTime:yyyy-MM-dd HH:mm:ss}");

            if (stat.Warnings.Count != 0)
            {
                content.AppendLine("Warnings:");
                foreach (string? warning in stat.Warnings) content.AppendLine($"  - {warning}");
            }

            if (stat.Errors.Count != 0)
            {
                content.AppendLine("Errors:");
                foreach (string? error in stat.Errors) content.AppendLine($"  - {error}");
            }

            content.AppendLine();
        }

        await File.WriteAllTextAsync(resultsPath, content.ToString());
        logger.LogInformation("Results saved to {Path}", resultsPath);
    }
}