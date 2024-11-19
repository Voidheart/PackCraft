using PackCraft.Models.TexturePack;

namespace PackCraft.Services.Lua;

public interface ILuaScriptGenerator
{
    Task GenerateLuaScriptAsync(string outputPath, Dictionary<string, SpriteData> spritesData);
}