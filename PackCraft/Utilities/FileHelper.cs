namespace PackCraft.Utilities;

public static class FileHelper
{
    private const string TextureExt = ".png";

    public static string AppendTextureExt(string path)
    {
        return path.EndsWith(TextureExt, StringComparison.OrdinalIgnoreCase)
            ? path
            : path + TextureExt;
    }

    public static string GetDataPath(string filePath, string? customDataPath, string? inputPath = null)
    {
        if (!string.IsNullOrEmpty(customDataPath)) return customDataPath;

        string? jsonPath = $"{filePath}.json";

        if (inputPath == null) return File.Exists(jsonPath) ? jsonPath : filePath;
        
        string? fullPath = Path.Combine(inputPath, jsonPath);
        if (File.Exists(fullPath)) return jsonPath;

        return File.Exists(jsonPath) ? jsonPath : filePath;
    }
}