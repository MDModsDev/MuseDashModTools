global using System.Collections.Generic;
global using System.Diagnostics;
global using System.Diagnostics.CodeAnalysis;
global using System.Globalization;
global using System.Linq;
global using System.Runtime.InteropServices;
global using System.Runtime.Versioning;
global using System.Threading;
global using System.Threading.Tasks;
global using Autofac;
global using Avalonia;
global using Avalonia.Controls;
global using Avalonia.Data.Converters;
global using Avalonia.Threading;
global using CommunityToolkit.Mvvm.ComponentModel;
global using CommunityToolkit.Mvvm.Input;
global using Downloader;
global using JetBrains.Annotations;
global using Microsoft.Extensions.Logging;
global using MuseDashModTools.Abstractions;
global using MuseDashModTools.Common;
global using MuseDashModTools.Common.Extensions;
global using MuseDashModTools.Core.Extensions;
global using MuseDashModTools.Extensions;
global using MuseDashModTools.Models;
global using MuseDashModTools.Models.Controls;
global using MuseDashModTools.Models.Enums;
global using MuseDashModTools.Models.GitHub;
global using MuseDashModTools.Services;
global using MuseDashModTools.Utils;
global using MuseDashModTools.ViewModels;
global using MuseDashModTools.ViewModels.Dialogs;
global using MuseDashModTools.ViewModels.Pages;
global using MuseDashModTools.ViewModels.Panels.Charting;
global using MuseDashModTools.ViewModels.Panels.Modding;
global using MuseDashModTools.ViewModels.Panels.Setting;
global using MuseDashModTools.Views;
global using MuseDashModTools.Views.Dialogs;
global using MuseDashModTools.Views.Pages;
global using MuseDashModTools.Views.Panels.Charting;
global using MuseDashModTools.Views.Panels.Modding;
global using MuseDashModTools.Views.Panels.Setting;
global using Ursa.Controls;
global using ZLogger;
global using static MuseDashModTools.Common.BuildInfo;
global using static MuseDashModTools.Common.PageNames;
global using static MuseDashModTools.Core.Utils.DesktopUtils;
global using static MuseDashModTools.Localization.General.Resources;
global using static MuseDashModTools.Localization.MsgBox.Resources;
global using static MuseDashModTools.Localization.XAML.Resources;