using System.Text.Json;
using Microsoft.Extensions.Logging;
using PackCraft.Models.TexturePack;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PackCraft.Services.Sprite;

public class SpriteExtractor(ApplicationContext context, ILogger<SpriteExtractor> logger)
    : ISpriteExtractor
{
    private readonly ApplicationContext _context = context;

    public async Task ExtractAllSpritesAsync(string texturePath, string dataPath, string outputPath,
        List<string> frameNameCriteria)
    {
        Dictionary<string, SpriteData>? spritesData =
            await GetFilteredSpritesDataAsync(texturePath, dataPath, frameNameCriteria);
        using Image<Rgba32>? texture = await Image.LoadAsync<Rgba32>(texturePath);
        List<Task>? tasks = new();

        foreach ((string? filename, SpriteData? spriteData) in spritesData)
        {
            string[]? parts = filename.Split('/');
            if (parts.Length < 3)
            {
                logger.LogError($"Invalid frame format: {filename}");
                continue;
            }

            string? race = parts[0];
            string? unitGroup = parts[1];
            string? frameName = Path.GetFileNameWithoutExtension(parts[2]);

            // Build dynamic output directory structure
            string? dynamicOutputPath = Path.Combine(outputPath, race, unitGroup);
            Directory.CreateDirectory(dynamicOutputPath);

            string? fileOut = Path.Combine(dynamicOutputPath, $"{frameName}.png");
            tasks.Add(ExtractSingleSpriteAsync(texture, spriteData, fileOut));
        }

        await Task.WhenAll(tasks);
        logger.LogInformation($"Extracted {spritesData.Count} sprites to '{outputPath}'");
    }

    public async Task<Dictionary<string, SpriteData>> GetFilteredSpritesDataAsync(string texturePath, string dataPath,
        List<string> frameNameCriteria)
    {
        string? rawData = await File.ReadAllTextAsync(dataPath);
        TexturePackerData? jsonData = JsonSerializer.Deserialize<TexturePackerData>(rawData);
        if (jsonData == null) throw new Exception("Failed to parse texture data file.");

        Dictionary<string, SpriteData>? spritesData = new();

        foreach ((string? filename, Frame? frame) in jsonData.Frames)
            if (frameNameCriteria.Count == 0 || frameNameCriteria.Any(criterion =>
                    filename.Contains(criterion, StringComparison.OrdinalIgnoreCase)))
            {
                FrameRect? f = frame.FrameRect;
                FrameSize? ss = frame.SourceSize;
                SpriteSourceSize? sss = frame.SpriteSourceSize;
                int w = f.W;
                int h = f.H;
                int x = !frame.Rotated ? f.X : f.Y;
                int y = !frame.Rotated ? f.Y : jsonData.Meta.Size.W - h - f.X;

                spritesData[filename] = new SpriteData
                {
                    Rotated = frame.Rotated,
                    ExtractRegion = new Region
                    {
                        Left = x,
                        Top = y,
                        Width = w,
                        Height = h
                    },
                    ExtendOptions = new ExtendOptions
                    {
                        Left = sss.X,
                        Top = sss.Y,
                        Right = frame.Trimmed ? ss.W - sss.W - sss.X : 0,
                        Bottom = frame.Trimmed ? ss.H - sss.H - sss.Y : 0
                    }
                };
            }

        return spritesData;
    }

    private async Task ExtractSingleSpriteAsync(Image<Rgba32> texture, SpriteData spriteData, string outputPath)
    {
        try
        {
            using Image<Rgba32>? clone = texture.Clone(ctx =>
            {
                if (spriteData.Rotated) ctx.Rotate(-90);

                Region? region = spriteData.ExtractRegion;
                ctx.Crop(new Rectangle(region.Left, region.Top, region.Width, region.Height));

                ExtendOptions? extend = spriteData.ExtendOptions;
                int newWidth = region.Width + extend.Left + extend.Right;
                int newHeight = region.Height + extend.Top + extend.Bottom;

                ctx.Pad(newWidth, newHeight, Color.Transparent);
            });

            await clone.SaveAsync(outputPath);
            logger.LogInformation($"Generated '{outputPath}'.");
        }
        catch (Exception ex)
        {
            logger.LogError($"Error processing '{outputPath}': {ex.Message}");
        }
    }
}