namespace MuseDashModToolsUI.Services.Downloads;

public abstract class GitHubServiceBase
{
    protected const string ReleaseApiUrl = "https://api.github.com/repos/MDModsDev/MuseDashModToolsUI/releases";
    protected const string LatestReleaseApiUrl = "https://api.github.com/repos/MDModsDev/MuseDashModToolsUI/releases/latest";
    protected const string ModLinksBaseUrl = "MDModsDev/ModLinks/main/";
    protected const string MelonLoaderBaseUrl = "LavaGang/MelonLoader/releases/download/v0.6.1/MelonLoader.x64.zip";
}