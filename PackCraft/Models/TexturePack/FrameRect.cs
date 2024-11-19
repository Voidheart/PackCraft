using System.Text.Json.Serialization;

namespace PackCraft.Models.TexturePack;

public class FrameRect
{
    [JsonPropertyName("x")] public int X { get; set; }

    [JsonPropertyName("y")] public int Y { get; set; }

    [JsonPropertyName("w")] public int W { get; set; }

    [JsonPropertyName("h")] public int H { get; set; }
}