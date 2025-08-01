﻿namespace MuseDashModTools.Core.Extensions;

public static class CoreServiceExtensions
{
    public static void RegisterLogger(this IServiceCollection services, string logFileName)
    {
        services.AddLogging(x =>
        {
            x.ClearProviders();
#if DEBUG
            x.SetMinimumLevel(LogLevel.Trace);
            x.AddZLoggerConsole(options =>
            {
                options.ConfigureEnableAnsiEscapeCode = true;
                options.UseFormatter(() => new LogConsoleFormatter());
            });
#else
            x.SetMinimumLevel(LogLevel.Information);
#endif
            x.AddZLoggerFile((options, _) =>
            {
                options.FileShared = true;
                options.UseFormatter(() => new LogFileFormatter());
                return Path.Combine("Logs", logFileName);
            });
        });
    }

    public static void RegisterInstances(this ContainerBuilder builder)
    {
        builder.RegisterInstance(new HttpClient());
        builder.RegisterInstance(new MultiThreadDownloader(
            new DownloadConfiguration
            {
                ChunkCount = 8,
                MaxTryAgainOnFailure = 4,
                ParallelCount = 4,
                ParallelDownload = true,
                Timeout = 3000
            }));
    }

    public static void RegisterCoreServices(this ContainerBuilder builder)
    {
        builder.RegisterType<Config>().SingleInstance();
        builder.RegisterType<WindowNotificationManager>().SingleInstance();

        builder.RegisterType<ChartManageService>().As<IChartManageService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<FileSystemService>().As<IFileSystemService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<FileSystemPickerService>().As<IFileSystemPickerService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<GameService>().As<IGameService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<JsonSerializationService>().As<IJsonSerializationService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<LocalService>().As<ILocalService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<MessageBoxService>().As<IMessageBoxService>().SingleInstance();
        builder.RegisterType<ModManageService>().As<IModManageService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<NotificationService>().As<INotificationService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<ResourceService>().As<IResourceService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<SettingService>().As<ISettingService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<UpdateService>().As<IUpdateService>().PropertiesAutowired().SingleInstance();

        // Download Services
        builder.RegisterType<CustomDownloadService>().As<ICustomDownloadService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<GiteeDownloadService>().As<IGiteeDownloadService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<GitHubDownloadService>().As<IGitHubDownloadService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<GitHubMirrorDownloadService>().As<IGitHubMirrorDownloadService>().PropertiesAutowired().SingleInstance();
        builder.RegisterType<DownloadManager>().As<IDownloadManager>().PropertiesAutowired().SingleInstance();

        // Platform Service
#if WINDOWS
        builder.RegisterType<WindowsService>().As<IPlatformService>().PropertiesAutowired().SingleInstance();
#elif LINUX
        builder.RegisterType<LinuxService>().As<IPlatformService>().PropertiesAutowired().SingleInstance();
#elif MACOS
        builder.RegisterType<MacOsService>().As<IPlatformService>().PropertiesAutowired().SingleInstance();
#endif
    }
}