﻿using MCM.Abstractions.Settings.Base.Global;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;

namespace AllegianceOverhaul
{
  public partial class Settings : AttributeGlobalSettings<Settings>
  {
    private const string HeadingMigrationTweaks = "{=cMaAPsPG7}Migration tweaks";

    //Ensured loyalty
    [SettingPropertyBool("{=cMaAPsPG7}Migration tweaks", RequireRestart = true, IsToggle = true, HintText = "{=WCoiQxsow}Enables a variety of tweaks regarding clans migration between kingdoms. Due to the nature of these changes, they always apply to every clan in the game.")]
    [SettingPropertyGroup(HeadingMigrationTweaks, GroupOrder = 1)]
    public bool UseMigrationTweaks { get; set; } = false;

    [SettingPropertyBool("{=13Y8NuKUS}Allow join requests", Order = 0, RequireRestart = false, HintText = "{=4ymcIHKvE}Specify if AI clans should consider player kingdom when they are deciding which kingdom to join or defect to.")]
    [SettingPropertyGroup(HeadingMigrationTweaks)]
    public bool AllowJoinRequests { get; set; } = true;

    [SettingPropertyBool("{=taw7SjXfx}Allow hire requests", Order = 1, RequireRestart = false, HintText = "{=wb3y7btgb}Specify if AI clans should consider player kingdom when they are deciding which kingdom to join as mercenaries.")]
    [SettingPropertyGroup(HeadingMigrationTweaks)]
    public bool AllowHireRequests { get; set; } = true;

    //Determined kingdom pick
    [SettingPropertyBool("{=ZZKClL1sA}Determined kingdom pick logic", Order = 2, RequireRestart = false, HintText = "{=OUEwq5kM0}Specify if AI clans should always pick the most attractive kingdom instead of a random one when considering joining some kingdom or defecting. Enabling this can potentially increase intensity of clan migration between kingdoms.")]
    [SettingPropertyGroup(HeadingMigrationTweaks)]
    public bool UseDeterminedKingdomPick { get; set; } = false;
  }
}
