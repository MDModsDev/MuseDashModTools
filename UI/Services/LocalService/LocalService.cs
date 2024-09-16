using System.IO.Compression;
using System.Text;
using AsmResolver.DotNet;
using CliWrap;

namespace MuseDashModToolsUI.Services;

public sealed partial class LocalService : ILocalService
{
    public async Task CheckDotNetRuntimeInstallAsync()
    {
        var outputStringBuilder = new StringBuilder();
        await Cli.Wrap("dotnet")
            .WithArguments("--list-runtimes")
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(outputStringBuilder))
            .ExecuteAsync()
            .ConfigureAwait(false);

        if (!outputStringBuilder.ToString().Contains("Microsoft.WindowsDesktop.App 6."))
        {
            Logger.Information("DotNet Runtime not found, showing error message box...");
            await ErrorMessageBoxAsync(MsgBox_Content_DotNetRuntimeNotFound).ConfigureAwait(true);
        }
    }

    public IEnumerable<string> GetModFiles(string folderPath) => Directory.GetFiles(folderPath)
        .Where(x => Path.GetExtension(x) == ".disabled" || Path.GetExtension(x) == ".dll");

    public async Task<string> GetMuseDashFolderAsync()
    {
        var path = string.Empty;

        while (path.IsNullOrEmpty() || !await CheckValidPathAsync(path).ConfigureAwait(true))
        {
            path = await FileSystemPickerService.GetSingleFolderPathAsync(FolderDialog_Title_ChooseMuseDashFolder).ConfigureAwait(true);
            Logger.Information("Selected MuseDash folder: {MuseDashFolder}", path);
        }

        return path;
    }

    public async Task<ModDto?> LoadModFromPathAsync(string filePath)
    {
        var mod = new ModDto
        {
            FileName = Path.GetFileNameWithoutExtension(filePath),
            IsDisabled = Path.GetExtension(filePath) == ".disabled"
        };

        var module = ModuleDefinition.FromFile(filePath);
        var attribute = module.Assembly!.FindCustomAttributes("MelonLoader", "MelonInfoAttribute").Single();
        mod.Name = attribute.Signature!.FixedArguments[1].ToString();
        mod.Version = attribute.Signature!.FixedArguments[2].ToString();
        mod.Author = attribute.Signature!.FixedArguments[3].ToString();
        mod.SHA256 = await HashUtils.ComputeSHA256HashFromPathAsync(filePath).ConfigureAwait(false);

        return mod;
    }

    public void LaunchGame(bool isModded)
    {
        var launchArguments = new StringBuilder();
        if (!isModded)
        {
            launchArguments.Append("//--no-mods");
        }
        else if (!Setting.ShowConsole)
        {
            launchArguments.Append("//--melonloader.hideconsole");
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = "steam://rungameid/774171" + launchArguments,
            UseShellExecute = true
        });

        Logger.Information("Launching game with launch arguments: {LaunchArguments}", launchArguments);
    }

    public bool UnzipFile(string zipPath, string extractPath)
    {
        try
        {
            ZipFile.ExtractToDirectory(zipPath, extractPath, true);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to unzip file {ZipPath} to {ExtractPath}", zipPath, extractPath);
            return false;
        }
    }

    #region Injections

    [UsedImplicitly]
    public ILogger Logger { get; init; } = null!;

    [UsedImplicitly]
    public Setting Setting { get; init; } = null!;

    [UsedImplicitly]
    public IFileSystemPickerService FileSystemPickerService { get; init; } = null!;

    #endregion Injections
}