using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Resources;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Localization;
using MuseDashModToolsUI.Models;
using Serilog;
using static MuseDashModToolsUI.Localization.Resources;

namespace MuseDashModToolsUI.Services;

public class LocalizationService : ILocalizationService, INotifyPropertyChanged
{
    private readonly ILogger _logger;
    private readonly Lazy<IMainWindowViewModel> _mainWindowViewModel;
    private readonly ISettingService _settingService;
    private readonly Lazy<ISettingsViewModel> _settingsViewModel;

    public string this[string resourceKey] =>
        Resources.ResourceManager.GetString(resourceKey, Culture)?.Replace("\\n", "\n") ?? $"#{resourceKey}#";

    public LocalizationService(ILogger logger, ISettingService settingService, Lazy<IMainWindowViewModel> mainWindowViewModel,
        Lazy<ISettingsViewModel> settingsViewModel)
    {
        _logger = logger;
        _settingService = settingService;
        _mainWindowViewModel = mainWindowViewModel;
        _settingsViewModel = settingsViewModel;
        GetAvailableCultures();
    }

    public List<Language> AvailableLanguages { get; } = new();

    public void SetLanguage(string language)
    {
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(language);
        _settingService.Settings.LanguageCode = language;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        _mainWindowViewModel.Value.ChangeTabName();
        _settingsViewModel.Value.ChangeOptionName();
        _logger.Information("Language changed to {Language}", language);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void GetAvailableCultures()
    {
        var rm = new ResourceManager(typeof(Resources));
        var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
        var defaultCulture = CultureInfo.GetCultureInfo("en");


        foreach (var culture in cultures)
        {
            if (culture.Equals(CultureInfo.InvariantCulture))
            {
                AvailableLanguages.Add(new Language(defaultCulture.Name, defaultCulture.DisplayName));
                continue;
            }

            var rs = rm.GetResourceSet(culture, true, false);
            if (rs != null)
                AvailableLanguages.Add(new Language(culture.Name, culture.DisplayName));
        }

        _logger.Information("Available languages loaded: {AvailableLanguages}",
            string.Join(", ", AvailableLanguages.Select(x => x.Name)));
    }
}