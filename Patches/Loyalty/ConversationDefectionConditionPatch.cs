﻿using AllegianceOverhaul.Helpers;
using AllegianceOverhaul.LoyaltyRebalance;

using HarmonyLib;

using System;
using System.Reflection;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Localization;

namespace AllegianceOverhaul.Patches.Loyalty
{
    [HarmonyPatch(typeof(LordDefectionCampaignBehavior), "conversation_lord_from_ruling_clan_on_condition")]
    public static class ConversationDefectionConditionPatch
    {
        private const string LoyaltyRefuse = "{=5lThJhdEu}I have no will to discuss such matters with outsiders.";
        private const string LoyaltyPoliteRefuse = "{=P0bzqgjMT}I don't think this conversation will take us anywhere.";
        private const string LoyaltyFriendlyRefuse = "{=K9L6J9ZIy}I'm sorry, my friend, but I don't think this conversation will take us anywhere.";
        public static void Postfix(ref bool __result)
        {
            try
            {
                if (SettingsHelper.SubSystemEnabled(SubSystemType.LoyaltyInConversations, Hero.OneToOneConversationHero.Clan))
                {
                    if (!__result && LoyaltyRebalance.EnsuredLoyalty.LoyaltyManager.CheckLoyalty(Hero.OneToOneConversationHero.Clan, Clan.PlayerClan.Kingdom))
                    {
                        float RelationWithPlayer = Hero.OneToOneConversationHero.GetRelationWithPlayer();
                        if (RelationWithPlayer >= 30)
                            MBTextManager.SetTextVariable("LIEGE_IS_RELATIVE", new TextObject(LoyaltyFriendlyRefuse), false);
                        else
                            MBTextManager.SetTextVariable("LIEGE_IS_RELATIVE", new TextObject(!RelativesHelper.BloodRelatives(Hero.OneToOneConversationHero, Hero.MainHero) && RelationWithPlayer <= -10 ? LoyaltyRefuse : LoyaltyPoliteRefuse), false);
                        __result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MethodInfo? methodInfo = MethodBase.GetCurrentMethod() as MethodInfo;
                DebugHelper.HandleException(ex, methodInfo, "Harmony patch for conversation_lord_from_ruling_clan_on_condition");
            }
        }
        public static bool Prepare()
        {
            return SettingsHelper.SubSystemEnabled(SubSystemType.LoyaltyInConversations);
        }
    }
}