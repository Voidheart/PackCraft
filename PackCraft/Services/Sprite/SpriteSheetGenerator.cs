using Microsoft.Extensions.Logging;
using PackCraft.Models.TexturePack;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PackCraft.Services.Sprite;

public class SpriteSheetGenerator(ApplicationContext context, ILogger<SpriteExtractor> logger)
    : ISpriteSheetGenerator
{
    private const string OutputFileName = "sprite_sheet.png";

    private readonly ApplicationContext _context = context;

    public async Task GenerateSpriteSheetAsync(string texturePath, Dictionary<string, SpriteData> spritesData,
        string outputPath, float? scale)
    {
        // Group sprites by their directory path
        Dictionary<string, Dictionary<string, SpriteData>>? spriteGroups = spritesData
            .GroupBy(kvp => Path.GetDirectoryName(kvp.Key)!)
            .ToDictionary(g => g.Key, g => g.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

        foreach ((string? groupPath, Dictionary<string, SpriteData>? groupSprites) in spriteGroups)
            await GenerateGroupSpriteSheetAsync(texturePath, groupSprites, outputPath, groupPath, scale);
    }

    private async Task GenerateGroupSpriteSheetAsync(string texturePath, Dictionary<string, SpriteData> groupSprites,
        string baseOutputPath, string groupPath, float? scale)
    {
        using Image? texture = await Image.LoadAsync(texturePath);

        int frameWidth = groupSprites.Values.Max(s => s.ExtractRegion.Width);
        int frameHeight = groupSprites.Values.Max(s => s.ExtractRegion.Height);
        int frameCount = groupSprites.Count;

        if (scale.HasValue && scale.Value != 1.0f)
        {
            frameWidth = (int)(frameWidth * scale.Value);
            frameHeight = (int)(frameHeight * scale.Value);
        }

        int gridWidth = (int)Math.Ceiling(Math.Sqrt(frameCount));
        int gridHeight = (int)Math.Ceiling((double)frameCount / gridWidth);
        int sheetWidth = gridWidth * frameWidth;
        int sheetHeight = gridHeight * frameHeight;

        using Image<Rgba32>? spriteSheet = new(sheetWidth, sheetHeight);

        int i = 0;
        foreach ((string? filename, SpriteData? spriteData) in groupSprites.OrderBy(x => x.Key))
        {
            Region? region = spriteData.ExtractRegion;
            using Image? sprite = texture.Clone(ctx =>
            {
                ctx.Crop(new Rectangle(region.Left, region.Top, region.Width, region.Height));
                if (scale.HasValue && scale.Value != 1.0f)
                    ctx.Resize(new ResizeOptions
                    {
                        Size = new Size(frameWidth, frameHeight),
                        Mode = ResizeMode.Stretch
                    });
            });

            int x = i % gridWidth * frameWidth;
            int y = i / gridWidth * frameHeight;
            spriteSheet.Mutate(ctx => ctx.DrawImage(sprite, new Point(x, y), 1f));
            i++;
        }

        // Create the full output path for this sprite group
        string? groupOutputPath = Path.Combine(baseOutputPath, groupPath);
        Directory.CreateDirectory(groupOutputPath);

        string? outputFilePath = Path.Combine(groupOutputPath, OutputFileName);
        await spriteSheet.SaveAsync(outputFilePath);
        logger.LogInformation($"Sprite sheet saved to '{outputFilePath}'");
    }
}