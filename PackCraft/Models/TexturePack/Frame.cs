using System.Text.Json.Serialization;

namespace PackCraft.Models.TexturePack;

public class Frame
{
    [JsonPropertyName("frame")] public FrameRect FrameRect { get; set; } = new();

    [JsonPropertyName("rotated")] public bool Rotated { get; set; }

    [JsonPropertyName("trimmed")] public bool Trimmed { get; set; }

    [JsonPropertyName("spriteSourceSize")] public SpriteSourceSize SpriteSourceSize { get; set; } = new();

    [JsonPropertyName("sourceSize")] public FrameSize SourceSize { get; set; } = new();
}