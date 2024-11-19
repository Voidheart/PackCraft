using PackCraft.Models.TexturePack;

namespace PackCraft.Services.Sprite;

public interface ISpriteExtractor
{
    Task ExtractAllSpritesAsync(string texturePath, string dataPath, string outputPath, List<string> frameNameCriteria);

    Task<Dictionary<string, SpriteData>> GetFilteredSpritesDataAsync(string texturePath, string dataPath,
        List<string> frameNameCriteria);
}