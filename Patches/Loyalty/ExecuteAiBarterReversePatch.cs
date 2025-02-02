﻿using HarmonyLib;
using TaleWorlds.CampaignSystem;

namespace AllegianceOverhaul.Patches.Loyalty
{
    [HarmonyPatch(typeof(BarterManager), "ExecuteAiBarter", new[] { typeof(IFaction), typeof(IFaction), typeof(Hero), typeof(Hero), typeof(Barterable) })]
    public static class ExecuteAiBarterReversePatch
    {
        [HarmonyReversePatch]
        public static void ExecuteAiBarter(object instance, IFaction faction1, IFaction faction2, Hero faction1Hero, Hero faction2Hero, Barterable barterable)
        {
            // its a stub so it has no initial content
            throw new System.NotImplementedException("It's a stub");
        }
    }
}
