namespace Euterpe.Models.Charts;

public static class ChartFiles
{
    public const string ManifestExtension = ".epk";
    public const string ManifestFileName = $"manifest{ManifestExtension}";
    public const string PackageExtension = ".zip";
    public const string MusicFileName = $"{MusicName}{MusicExtension}";
    public const string DemoFileName = $"{DemoName}{MusicExtension}";
    public const string VideoFileName = "video.mp4";
    public const string CoverFileName = "cover.webp";

    public const string MusicName = "music";
    public const string DemoName = "demo";
    public const string MusicExtension = ".ogg";

    public const string MapPrefix = "map";
    public const string BmsExtension = ".bms";
    public const string TalkExtension = ".talk";

    public static bool IsLargeMedia(string fileName) => fileName == VideoFileName;

    public static bool IsChartFile(string fileName) =>
        fileName is ManifestFileName or MusicFileName or DemoFileName or VideoFileName or CoverFileName
        || IsMapFile(fileName);

    public static string MapName(ChartDifficulty difficulty) => $"{MapPrefix}{(int)difficulty}";

    public static string MapFileName(ChartDifficulty difficulty) => $"{MapName(difficulty)}{BmsExtension}";

    public static IReadOnlyList<ChartDifficulty> ExistingDifficulties(this IReadOnlyDictionary<string, ManifestFileEntry> files) =>
        [.. ChartDifficultyExtensions.GetValues().Where(difficulty => files.ContainsKey(MapFileName(difficulty)))];

    public static string? FindCoverPath(this IReadOnlyDictionary<string, ManifestFileEntry> files, string folderPath) =>
        files.ContainsKey(CoverFileName) ? Path.Combine(folderPath, CoverFileName) : null;

    private static bool IsMapFile(string fileName)
    {
        var extension = Path.GetExtension(fileName.AsSpan());
        if (!extension.Equals(BmsExtension, StringComparison.OrdinalIgnoreCase)
            && !extension.Equals(TalkExtension, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var stem = Path.GetFileNameWithoutExtension(fileName.AsSpan());
        return stem.StartsWith(MapPrefix, StringComparison.OrdinalIgnoreCase)
               && int.TryParse(stem[MapPrefix.Length..], out _);
    }
}
