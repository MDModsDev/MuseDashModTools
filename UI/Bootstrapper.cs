﻿using Autofac;
using MuseDashModToolsUI.Contracts.Serializations;
using MuseDashModToolsUI.Extensions.MarkupExtensions;
using MuseDashModToolsUI.Services.Downloads;
using MuseDashModToolsUI.ViewModels;
using MuseDashModToolsUI.ViewModels.Dialogs;
using MuseDashModToolsUI.ViewModels.Pages;

namespace MuseDashModToolsUI;

public static class Bootstrapper
{
    private static readonly ContainerBuilder _builder = new();

    /// <summary>
    ///     Register all instances, services and view models
    ///     Configure static resolvers
    /// </summary>
    public static void Register()
    {
        RegisterInstances();
        RegisterComponents();
        RegisterServices();
        RegisterViewModels();

        var container = _builder.Build();
        ConfigureStaticResolvers(container);
    }

    /// <summary>
    ///     Register instances
    /// </summary>
    private static void RegisterInstances()
    {
        _builder.RegisterInstance(Log.Logger).As<ILogger>().SingleInstance();
        _builder.RegisterInstance(new HttpClient());
        _builder.RegisterInstance(new MultiThreadDownloader(
            new DownloadConfiguration
            {
                ChunkCount = 8,
                MaxTryAgainOnFailover = 4,
                ParallelCount = 4,
                ParallelDownload = true,
                Timeout = 3000
            }));
    }

    /// <summary>
    ///     Register Frequently-Used Components
    /// </summary>
    private static void RegisterComponents()
    {
        _builder.RegisterType<Setting>().SingleInstance();
    }

    /// <summary>
    ///     Register all services
    /// </summary>
    private static void RegisterServices()
    {
        // _builder.RegisterType<ChartService>().As<IChartService>().PropertiesAutowired().SingleInstance();
        _builder.RegisterType<FileSystemPickerService>().As<IFileSystemPickerService>().PropertiesAutowired().SingleInstance();
        // _builder.RegisterType<InfoJsonService>().As<IInfoJsonService>().PropertiesAutowired().SingleInstance();
        _builder.RegisterType<LocalService>().As<ILocalService>().PropertiesAutowired().SingleInstance();
        // _builder.RegisterType<LogAnalyzeService>().As<ILogAnalyzeService>().PropertiesAutowired().SingleInstance();
        // _builder.RegisterType<ModService>().As<IModService>().PropertiesAutowired().SingleInstance();
        // _builder.RegisterType<NavigationService>().As<INavigationService>().PropertiesAutowired().SingleInstance();
        _builder.RegisterType<SavingService>().As<ISavingService>().PropertiesAutowired().SingleInstance();
        _builder.RegisterType<JsonSerializationService>().As<IJsonSerializationService>().PropertiesAutowired().SingleInstance();
        // _builder.RegisterType<UpdateUIService>().As<IUpdateUIService>().PropertiesAutowired().SingleInstance();

        // Download Services
        _builder.RegisterType<CustomDownloadService>().As<ICustomDownloadService>().PropertiesAutowired().SingleInstance();
        _builder.RegisterType<GitHubDownloadService>().As<IGitHubDownloadService>().PropertiesAutowired().SingleInstance();
        _builder.RegisterType<GitHubMirrorDownloadService>().As<IGitHubMirrorDownloadService>().PropertiesAutowired().SingleInstance();
        _builder.RegisterType<DownloadManager>().As<IDownloadManager>().PropertiesAutowired().SingleInstance();

        // Platform Service
        if (OperatingSystem.IsWindows())
        {
            _builder.RegisterType<WindowsService>().As<IPlatformService>().PropertiesAutowired().SingleInstance();
        }
        else if (OperatingSystem.IsLinux())
        {
            _builder.RegisterType<LinuxService>().As<IPlatformService>().PropertiesAutowired().SingleInstance();
        }
        // for macos development
        else if (OperatingSystem.IsMacOS())
        {
            _builder.RegisterType<MacOsService>().As<IPlatformService>().PropertiesAutowired().SingleInstance();
        }
    }

    /// <summary>
    ///     Register all view models
    /// </summary>
    private static void RegisterViewModels()
    {
        _builder.RegisterType<AboutViewModel>().As<IAboutViewModel>().PropertiesAutowired().SingleInstance();
        _builder.RegisterType<ChartDownloadViewModel>().As<IChartDownloadViewModel>().PropertiesAutowired().SingleInstance();
        _builder.RegisterType<DownloadWindowViewModel>().As<IDownloadWindowViewModel>().PropertiesAutowired().SingleInstance();
        _builder.RegisterType<InfoJsonViewModel>().As<IInfoJsonViewModel>().PropertiesAutowired().SingleInstance();
        _builder.RegisterType<LogAnalysisViewModel>().As<ILogAnalysisViewModel>().PropertiesAutowired().SingleInstance();
        _builder.RegisterType<ModManageViewModel>().As<IModManageViewModel>().PropertiesAutowired().SingleInstance();
        _builder.RegisterType<SettingsViewModel>().As<ISettingsViewModel>().PropertiesAutowired().SingleInstance();
        _builder.RegisterType<MainMenuViewModel>().As<IMainMenuViewModel>().PropertiesAutowired().SingleInstance();
        _builder.RegisterType<MainWindowViewModel>().As<IMainWindowViewModel>().PropertiesAutowired().SingleInstance();
    }

    /// <summary>
    ///     Configure static resolvers
    /// </summary>
    /// <param name="container"></param>
    private static void ConfigureStaticResolvers(IComponentContext container)
    {
        DependencyInjectionExtension.Resolver = type => container.Resolve(type!);
    }
}