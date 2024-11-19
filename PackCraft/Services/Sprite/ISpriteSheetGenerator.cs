using PackCraft.Models.TexturePack;

namespace PackCraft.Services.Sprite;

public interface ISpriteSheetGenerator
{
    Task GenerateSpriteSheetAsync(string texturePath, Dictionary<string, SpriteData> spritesData, string outputPath,
        float? scale);
}