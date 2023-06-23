using System;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using MuseDashModToolsUI.Contracts;

namespace MuseDashModToolsUI.Extensions;

public class LocalizeExtensions : MarkupExtension
{
    private string? Key { get; }
    public ILocalizationService? LocalizationService { get; init; }

    public LocalizeExtensions(string key) => Key = key;

    public LocalizeExtensions()
    {
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        /*var n = LocalizationService is not null;
        Log.Logger.Information("{N}", n);*/
        return new Binding
        {
            Mode = BindingMode.OneWay,
            Source = LocalizationService,
            Path = $"[{Key}]"
        };
    }
}