global using System.Collections.Generic;
global using System.Diagnostics;
global using System.Linq;
global using System.Runtime.InteropServices;
global using System.Runtime.Versioning;
global using System.Threading;
global using System.Threading.Tasks;
global using Avalonia;
global using Avalonia.Controls;
global using Avalonia.Threading;
global using CommunityToolkit.Mvvm.ComponentModel;
global using CommunityToolkit.Mvvm.Input;
global using JetBrains.Annotations;
global using MuseDashModToolsUI.Contracts;
global using MuseDashModToolsUI.Contracts.Downloads;
global using MuseDashModToolsUI.Contracts.ViewModels;
global using MuseDashModToolsUI.Extensions;
global using MuseDashModToolsUI.Formatters;
global using MuseDashModToolsUI.Models;
global using MuseDashModToolsUI.Models.DTOs;
global using MuseDashModToolsUI.Models.Enums;
global using MuseDashModToolsUI.Services;
global using MuseDashModToolsUI.Utils;
global using MuseDashModToolsUI.Views;
global using Semver;
global using Serilog;
global using Ursa.Controls;
global using static MuseDashModToolsUI.BuildInfo;
global using static MuseDashModToolsUI.Localization.General.Resources;
global using static MuseDashModToolsUI.Localization.MsgBox.Resources;
global using static MuseDashModToolsUI.Localization.XAML.Resources;
global using static MuseDashModToolsUI.Utils.DesktopUtils;
global using static MuseDashModToolsUI.Utils.MessageBoxUtils;
global using MultiThreadDownloader = Downloader.DownloadService;