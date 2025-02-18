﻿using DynamicData;

namespace MuseDashModTools.Abstractions;

public interface IModManageService
{
    Task InitializeModsAsync(SourceCache<ModDto, string> sourceCache);
    Task InstallModAsync(ModDto mod);
    Task UninstallModAsync(ModDto mod);
    Task UpdateModAsync(ModDto mod);
    Task ToggleModAsync(ModDto mod);
}