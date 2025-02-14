﻿global using System.Diagnostics;
global using System.Diagnostics.CodeAnalysis;
global using System.Globalization;
global using System.Runtime.InteropServices;
global using System.Runtime.Versioning;
global using Autofac;
global using Avalonia.Controls;
global using Downloader;
global using JetBrains.Annotations;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using MuseDashModTools.Abstractions;
global using MuseDashModTools.Common.Extensions;
global using MuseDashModTools.Core.Extensions;
global using MuseDashModTools.Core.Proxies;
global using MuseDashModTools.Core.Utils;
global using MuseDashModTools.Models;
global using MuseDashModTools.Models.Enums;
global using MuseDashModTools.Models.GitHub;
global using Semver;
global using Ursa.Controls;
global using ZLogger;
global using static MuseDashModTools.Common.BuildInfo;
global using static MuseDashModTools.Common.GitHubConstants;
global using static MuseDashModTools.Common.GitHubResources;
global using static MuseDashModTools.Core.Utils.DesktopUtils;
global using static MuseDashModTools.Localization.General.Resources;
global using static MuseDashModTools.Localization.MsgBox.Resources;
global using IDownloadService = MuseDashModTools.Abstractions.IDownloadService;
global using MultiThreadDownloader = Downloader.DownloadService;