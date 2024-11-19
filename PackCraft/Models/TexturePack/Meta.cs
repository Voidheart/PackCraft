using System.Text.Json.Serialization;

namespace PackCraft.Models.TexturePack;

public class Meta
{
    [JsonPropertyName("size")] public FrameSize Size { get; set; } = new();
}