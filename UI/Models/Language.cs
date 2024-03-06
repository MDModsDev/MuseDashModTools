﻿using System.Globalization;

namespace MuseDashModToolsUI.Models;

public sealed class Language(CultureInfo cultureInfo)
{
    public string? Name { get; } = cultureInfo.Name;
    private string? DisplayName { get; } = cultureInfo.DisplayName;
    public string FullName => $"{Name} - {DisplayName}";
}