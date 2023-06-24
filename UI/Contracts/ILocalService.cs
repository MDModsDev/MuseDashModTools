﻿using System.Collections.Generic;
using System.Threading.Tasks;
using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Contracts;

public interface ILocalService
{
    IEnumerable<string> GetModFiles(string path);
    Mod? LoadMod(string filePath);
    Task<bool> CheckValidPath();
    Task<string> ReadGameVersion();
    Task CheckMelonLoaderInstall();
    void OnInstallMelonLoader();
    Task OnUninstallMelonLoader();
    Task OpenModsFolder();
    Task OpenUserDataFolder();
}