using System.Text.Json.Serialization;

namespace PackCraft.Models.TexturePack;

public class TexturePackerData
{
    [JsonPropertyName("frames")] public Dictionary<string, Frame> Frames { get; set; } = new();

    [JsonPropertyName("meta")] public Meta Meta { get; set; } = new();
}