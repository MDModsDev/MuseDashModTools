global using System.Collections.Generic;
global using System.Diagnostics;
global using System.Diagnostics.CodeAnalysis;
global using System.Globalization;
global using System.Linq;
global using System.Runtime.InteropServices;
global using System.Runtime.Versioning;
global using System.Threading;
global using System.Threading.Tasks;
global using Avalonia;
global using Avalonia.Controls;
global using Avalonia.Data.Converters;
global using Avalonia.Threading;
global using CommunityToolkit.Mvvm.ComponentModel;
global using CommunityToolkit.Mvvm.Input;
global using Downloader;
global using JetBrains.Annotations;
global using MuseDashModTools.Contracts;
global using MuseDashModTools.Extensions;
global using MuseDashModTools.Formatters;
global using MuseDashModTools.Models;
global using MuseDashModTools.Models.Enums;
global using MuseDashModTools.Models.GitHub;
global using MuseDashModTools.Services;
global using MuseDashModTools.Utils;
global using MuseDashModTools.ViewModels;
global using MuseDashModTools.ViewModels.Dialogs;
global using MuseDashModTools.ViewModels.Pages;
global using MuseDashModTools.Views;
global using MuseDashModTools.Views.Pages;
global using Semver;
global using Serilog;
global using Ursa.Controls;
global using static MuseDashModTools.BuildInfo;
global using static MuseDashModTools.Localization.General.Resources;
global using static MuseDashModTools.Localization.MsgBox.Resources;
global using static MuseDashModTools.Localization.XAML.Resources;
global using static MuseDashModTools.Models.Constants.GitHubConstants;
global using static MuseDashModTools.Models.Constants.PageNames;
global using static MuseDashModTools.Utils.DesktopUtils;
global using static MuseDashModTools.Utils.MessageBoxUtils;
global using MultiThreadDownloader = Downloader.DownloadService;