using Microsoft.Extensions.Logging;
using PackCraft.Models;
using PackCraft.Models.TexturePack;
using PackCraft.Options;
using PackCraft.Services.Lua;
using PackCraft.Services.Sprite;
using PackCraft.Services.Statistics;
using PackCraft.Utilities;
using SixLabors.ImageSharp;

namespace PackCraft.Services.Texture;

public class TextureUnpackerService(
    ISpriteSheetGenerator spriteSheetGenerator,
    ILuaScriptGenerator luaScriptGenerator,
    ISpriteExtractor spriteExtractor,
    IStatisticsService statisticsService,
    ApplicationContext context,
    ILogger<TextureUnpackerService> logger)
    : ITextureUnpackerService
{
    private bool _outputCleaned;

    public async Task ProcessTexturesAsync()
    {
        try
        {
            UnpackOptions? options = context.UnpackOptions;
            string? absoluteInputPath = Path.GetFullPath(options.InputPath);
            string? absoluteDataPath = options.DataPath != null ? Path.GetFullPath(options.DataPath) : null;
            string? absoluteOutputPath = options.OutputPath != null ? Path.GetFullPath(options.OutputPath) : null;

            // Clean output directory once at the start if requested
            if (options.Clean && !_outputCleaned && absoluteOutputPath != null)
            {
                await CleanOutputDirectoryAsync(absoluteOutputPath);
                _outputCleaned = true;
            }

            if (File.Exists(FileHelper.AppendTextureExt(absoluteInputPath)))
            {
                await ProcessSingleTextureAsync(absoluteInputPath, absoluteDataPath, absoluteOutputPath, options);
            }
            else if (Directory.Exists(absoluteInputPath))
            {
                string[]? textureFiles = GetTextureFiles(absoluteInputPath);
                foreach (string? file in textureFiles)
                {
                    string? relativeFilePath = Path.GetRelativePath(absoluteInputPath, file);
                    string? fileNameWithoutExt = Path.GetFileNameWithoutExtension(relativeFilePath);
                    await ProcessSingleTextureAsync(
                        Path.Combine(absoluteInputPath, fileNameWithoutExt),
                        absoluteDataPath,
                        absoluteOutputPath,
                        options
                    );
                }
            }
            else
            {
                logger.LogError("Input path '{Path}' does not exist", absoluteInputPath);
                throw new DirectoryNotFoundException($"Input path '{absoluteInputPath}' does not exist.");
            }

            await statisticsService.SaveResultsAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during texture unpacking");
            throw;
        }
    }

    private async Task CleanOutputDirectoryAsync(string outputPath)
    {
        if (Directory.Exists(outputPath))
        {
            logger.LogInformation("Cleaning output directory: {Path}", outputPath);
            Directory.Delete(outputPath, true);
        }

        Directory.CreateDirectory(outputPath);
    }

    public async Task ProcessSingleTextureAsync(string texturePathWithoutExt, string? dataPath, string? outputPath,
        UnpackOptions options)
    {
        string? fullTexturePath = FileHelper.AppendTextureExt(texturePathWithoutExt);
        string? dataFilePath = FileHelper.GetDataPath(
            Path.GetFileNameWithoutExtension(fullTexturePath),
            dataPath,
            Path.GetDirectoryName(fullTexturePath));
        string? fullDataPath = Path.Combine(Path.GetDirectoryName(fullTexturePath)!, dataFilePath);

        logger.LogInformation("Processing texture: {Texture}", fullTexturePath);
        logger.LogInformation("Using data file: {Data}", fullDataPath);

        if (!File.Exists(fullTexturePath) || !File.Exists(fullDataPath))
        {
            string? error = $"Missing required files: Texture: {fullTexturePath}, Data: {fullDataPath}";
            logger.LogError(error);
            throw new FileNotFoundException(error);
        }

        string? finalOutputPath = outputPath ?? Path.GetDirectoryName(fullTexturePath)!;
        Directory.CreateDirectory(finalOutputPath);

        try
        {
            DateTime startTime = DateTime.Now;
            Dictionary<string, SpriteData>? spritesData = await spriteExtractor.GetFilteredSpritesDataAsync(
                fullTexturePath,
                fullDataPath,
                options.FrameNameCriteria);

            await spriteExtractor.ExtractAllSpritesAsync(
                fullTexturePath,
                fullDataPath,
                finalOutputPath,
                options.FrameNameCriteria);

            await spriteSheetGenerator.GenerateSpriteSheetAsync(
                fullTexturePath,
                spritesData,
                finalOutputPath,
                options.Scale);

            // Updated to pass spritesData to LuaScriptGenerator
            await luaScriptGenerator.GenerateLuaScriptAsync(finalOutputPath, spritesData);

            using Image? image = await Image.LoadAsync(fullTexturePath);
            await statisticsService.RecordStatisticsAsync(new ProcessingStatistics
            {
                SpriteSheetName = Path.GetFileName(fullTexturePath),
                Dimensions = new Size(image.Width, image.Height),
                TotalUnits = spritesData.Count,
                ProcessedUnits = spritesData.Count,
                ProcessingTime = startTime
            });

            logger.LogInformation("Finished processing '{Sheet}'",
                Path.GetFileNameWithoutExtension(fullTexturePath));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing '{Sheet}'",
                Path.GetFileNameWithoutExtension(fullTexturePath));
            throw;
        }
    }

    private string[] GetTextureFiles(string directoryPath)
    {
        return Directory.GetFiles(directoryPath, "*.png", SearchOption.AllDirectories);
    }
}