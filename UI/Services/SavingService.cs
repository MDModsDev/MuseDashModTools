namespace MuseDashModToolsUI.Services;

public sealed class SavingService : ISavingService
{
    private const string SettingFileName = "Setting.json";
    private static readonly string ConfigFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Name);
    private static readonly string SettingPath = Path.Combine(ConfigFolder, SettingFileName);

    public async Task LoadSettingAsync()
    {
        if (File.Exists(SettingPath))
        {
            await using var stream = new FileStream(SettingPath, FileMode.Open, FileAccess.Read);
            var savedSetting = await JsonSerializationService.DeserializeIndentedAsync<Setting>(stream).ConfigureAwait(false);
            if (savedSetting is null)
            {
                Logger.Error("Saved setting is null");
                return;
            }

            Setting.CopyFrom(savedSetting);
            Logger.Information("Setting loaded from {SettingPath} successfully", SettingPath);
        }
        else
        {
            Logger.Information("Setting file not found, using default settings");
        }
    }

    public async Task SaveSettingAsync()
    {
        await using var stream = new FileStream(SettingPath, FileMode.OpenOrCreate, FileAccess.Write);
        await JsonSerializationService.SerializeIndentedAsync(stream, Setting).ConfigureAwait(false);
        Logger.Information("Setting saved to {SettingPath} successfully", SettingPath);
    }

    #region Injections

    [UsedImplicitly]
    public IJsonSerializationService JsonSerializationService { get; init; } = null!;

    [UsedImplicitly]
    public ILogger Logger { get; init; } = null!;

    [UsedImplicitly]
    public Setting Setting { get; init; } = null!;

    #endregion Injections
}