using Avalonia.Styling;

namespace MuseDashModTools.Services;

public sealed partial class SavingService : ISavingService
{
    private const string SettingFileName = "Setting.json";
    private static readonly string ConfigFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);
    private static readonly string SettingPath = Path.Combine(ConfigFolder, SettingFileName);

    public async Task LoadSettingAsync()
    {
        if (File.Exists(SettingPath))
        {
            await using var stream = new FileStream(SettingPath, FileMode.Open, FileAccess.Read);
            var savedSetting = await JsonSerializationService.DeserializeIndentedAsync<Setting>(stream).ConfigureAwait(true);
            if (savedSetting is null)
            {
                Logger.Error("Saved setting is null");
                return;
            }

            Setting.CopyFrom(savedSetting);
            Logger.Information("Setting loaded from {SettingPath} successfully", SettingPath);
            await CheckValidSettingAsync().ConfigureAwait(true);
        }
        else
        {
            Logger.Information("Setting file not found, using default settings");
            await CheckValidSettingAsync().ConfigureAwait(true);
        }
    }

    public async Task SaveSettingAsync()
    {
        Setting.Theme = GetCurrentApplication().ActualThemeVariant == ThemeVariant.Light ? "Light" : "Dark";
        await using var stream = new FileStream(SettingPath, FileMode.OpenOrCreate, FileAccess.Write);
        await JsonSerializationService.SerializeIndentedAsync(stream, Setting).ConfigureAwait(false);
        Logger.Information("Setting saved to {SettingPath} successfully", SettingPath);
    }

    #region Injections

    [UsedImplicitly]
    public IJsonSerializationService JsonSerializationService { get; init; } = null!;

    [UsedImplicitly]
    public ILocalService LocalService { get; init; } = null!;

    [UsedImplicitly]
    public ILogger Logger { get; init; } = null!;

    [UsedImplicitly]
    public IPlatformService PlatformService { get; init; } = null!;

    [UsedImplicitly]
    public Setting Setting { get; init; } = null!;

    #endregion Injections
}