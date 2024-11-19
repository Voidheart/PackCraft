using SixLabors.ImageSharp;

namespace PackCraft.Models;

public class ProcessingStatistics
{
    public required string SpriteSheetName { get; init; }
    public required Size Dimensions { get; init; }
    public required int TotalUnits { get; init; }
    public required int ProcessedUnits { get; init; }
    public required DateTime ProcessingTime { get; init; }
    public List<string> Warnings { get; init; } = new();
    public List<string> Errors { get; init; } = new();
}