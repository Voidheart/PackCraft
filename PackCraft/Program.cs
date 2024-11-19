using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PackCraft;
using PackCraft.Options;
using PackCraft.Services.Lua;
using PackCraft.Services.Sprite;
using PackCraft.Services.Statistics;
using PackCraft.Services.Texture;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("../logs/packcraft.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();


// Configure services
ServiceProvider? serviceProvider = new ServiceCollection()
    .AddLogging(loggingBuilder =>
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddSerilog();
    })
    .AddSingleton<ApplicationContext>()
    .AddSingleton<ITextureUnpackerService, TextureUnpackerService>()
    .AddSingleton<ISpriteSheetGenerator, SpriteSheetGenerator>()
    .AddSingleton<ILuaScriptGenerator, LuaScriptGenerator>()
    .AddSingleton<ISpriteExtractor, SpriteExtractor>()
    .AddSingleton<IStatisticsService, StatisticsService>()
    .BuildServiceProvider();

// Configure command line options
Option<string>? inputOption = new(
    "--input-path",
    "Directory or sprite sheet path/name")
{
    IsRequired = true
};

Option<string>? dataFormatOption = new(
    "--data-format",
    description: "Data format type ('json')",
    getDefaultValue: () => "json");

Option<string>? dataPathOption = new(
    "--data-path",
    "Custom data file path");

Option<string>? outputOption = new(
    "--output-path",
    "Custom output directory path");

Option<bool>? cleanOption = new(
    "--clean",
    description: "Clean the output directory before unpacking",
    getDefaultValue: () => false);

Option<float?>? scaleOption = new(
    "--scale",
    "Scaling factor for output textures (e.g., 0.5 for half size)");

Option<string>? frameNameOption = new(
    "--frame-name",
    description:
    "Comma-separated list of frame name substrings to process (e.g., 'catapult,spearmen'). If empty, process all frames.",
    getDefaultValue: () => string.Empty);

Option<string>? resultsOption = new(
    "--results",
    "Custom path for the results file");

// Configure root command
RootCommand? rootCommand = new("Texture Unpacker for TexturePacker files")
{
    inputOption,
    dataFormatOption,
    dataPathOption,
    outputOption,
    cleanOption,
    scaleOption,
    frameNameOption,
    resultsOption
};

// Configure command handler
rootCommand.SetHandler(
    (Func<string, string, string, string, bool, float?, string, string, Task>)(async (input, format, dataPath, output,
        clean, scale, frameNames, results) =>
    {
        await ProcessCommand(input,
            format,
            dataPath,
            output,
            clean,
            scale,
            frameNames,
            results,
            serviceProvider);
    }),
    inputOption, dataFormatOption,
    dataPathOption, outputOption,
    cleanOption, scaleOption, frameNameOption,
    resultsOption);

try
{
    await rootCommand.InvokeAsync(args);
}
finally
{
    Log.CloseAndFlush();
    await serviceProvider.DisposeAsync();
}

static async Task ProcessCommand(string input,
    string format,
    string dataPath,
    string output,
    bool clean,
    float? scale,
    string frameNames,
    string results,
    ServiceProvider serviceProvider)
{
    try
    {
        ApplicationContext? appContext = serviceProvider.GetRequiredService<ApplicationContext>();
        ILogger<Program>? logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        appContext.UnpackOptions = new UnpackOptions
        {
            InputPath = input,
            DataFormat = format,
            DataPath = string.IsNullOrWhiteSpace(dataPath) ? null : dataPath,
            OutputPath = string.IsNullOrWhiteSpace(output) ? null : output,
            Clean = clean,
            Scale = scale,
            FrameNameCriteria = frameNames.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(name => name.Trim())
                .ToList(),
            ResultsPath = results
        };

        ITextureUnpackerService? unpackerService = serviceProvider.GetRequiredService<ITextureUnpackerService>();
        await unpackerService.ProcessTexturesAsync();
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while processing the texture pack");
        throw;
    }
}