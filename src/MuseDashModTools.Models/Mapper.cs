﻿using Riok.Mapperly.Abstractions;

namespace MuseDashModTools.Models;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public static partial class Mapper
{
    public static partial ModDto ToDto(this Mod mod);
    public static partial void UpdateFromMod([MappingTarget] this ModDto modDto, Mod mod);
    public static partial void CopyFrom([MappingTarget] this Setting currentSetting, Setting savedSetting);
}