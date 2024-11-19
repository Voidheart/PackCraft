namespace PackCraft.Options;

public class UnpackOptions
{
    public required string InputPath { get; set; }
    public string DataFormat { get; set; } = "json";
    public string? DataPath { get; set; }
    public string? OutputPath { get; set; }
    public bool Clean { get; set; } = false;
    public float? Scale { get; set; } = 1.0f;
    public List<string> FrameNameCriteria { get; set; } = new();
    public string? ResultsPath { get; set; }
}