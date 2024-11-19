using System.Text;
using Microsoft.Extensions.Logging;
using PackCraft.Models.TexturePack;

namespace PackCraft.Services.Lua;

public class LuaScriptGenerator : ILuaScriptGenerator
{
    private const string ScriptFileName = "sprite_animation.lua";
    private readonly ILogger<LuaScriptGenerator> _logger;

    public LuaScriptGenerator(ILogger<LuaScriptGenerator> logger)
    {
        _logger = logger;
    }

    public async Task GenerateLuaScriptAsync(string outputPath, Dictionary<string, SpriteData> spritesData)
    {
        // Group sprites by their directory path
        Dictionary<string, int>? spriteGroups = spritesData
            .GroupBy(kvp => Path.GetDirectoryName(kvp.Key)!)
            .ToDictionary(g => g.Key, g => g.Count());

        foreach ((string? groupPath, int frameCount) in spriteGroups)
            await GenerateGroupAnimationScriptAsync(outputPath, groupPath, frameCount);
    }

    private async Task GenerateGroupAnimationScriptAsync(string baseOutputPath, string groupPath, int frameCount)
    {
        string? groupName = Path.GetFileName(groupPath);
        StringBuilder? luaScript = new();

        luaScript.AppendLine($"DefineAnimations('{groupName}', {{");
        luaScript.AppendLine("  Still = {");

        for (int i = 0; i < frameCount; i++) luaScript.AppendLine($"    'frame {i}', 'wait 10',");

        luaScript.AppendLine("  },");
        luaScript.AppendLine("  Move = {");

        for (int i = 0; i < frameCount; i++) luaScript.AppendLine($"    'frame {i}', 'wait 5',");

        luaScript.AppendLine("  }");
        luaScript.AppendLine("});");

        // Create the full output path for this sprite group
        string? groupOutputPath = Path.Combine(baseOutputPath, groupPath);
        Directory.CreateDirectory(groupOutputPath);

        string? scriptPath = Path.Combine(groupOutputPath, ScriptFileName);
        await File.WriteAllTextAsync(scriptPath, luaScript.ToString());

        _logger.LogInformation("Generated animation script at '{Path}'", scriptPath);
    }
}