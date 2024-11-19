using System.Text.Json.Serialization;

namespace PackCraft.Models.TexturePack;

public class FrameSize
{
    [JsonPropertyName("w")] public int W { get; set; }

    [JsonPropertyName("h")] public int H { get; set; }
}