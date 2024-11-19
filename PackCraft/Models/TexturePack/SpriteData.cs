namespace PackCraft.Models.TexturePack;

public class SpriteData
{
    public bool Rotated { get; set; }
    public Region ExtractRegion { get; set; } = new();
    public ExtendOptions ExtendOptions { get; set; } = new();
}