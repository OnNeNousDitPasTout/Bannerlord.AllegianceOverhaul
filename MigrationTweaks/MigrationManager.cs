﻿extern alias TWCS;

using AllegianceOverhaul.Extensions;
using AllegianceOverhaul.Helpers;

using System;
using System.Reflection;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Localization;

using static AllegianceOverhaul.Helpers.LocalizationHelper;
using TWHelpers = TWCS::Helpers;

namespace AllegianceOverhaul.MigrationTweaks
{
    internal static class MigrationManager
    {
        private const string PlayerInquiryHeader = "{=0DEJNa14n}A clan wishes to join your kingdom!";
        private const string PlayerInquiryBody = "{=NAlAH3pJk}The {JOINING_CLAN.NAME} clan is looking to join your kingdom{?JOINING_CLAN.IS_MERCENARY} as a mercenary{?}{\\?}!{NEW_LINE} {NEW_LINE}They have {TROOPS_DESCRIPTION} and {FIEFS_DESCRIPTION}.Their leader is {RELATION} towards you.{NEW_LINE} {NEW_LINE}What say ye?";

        private const string TroopsDescription = "{=JhJW9KQcw}{TROOPS} {?TROOPS.PLURAL_FORM}able warriors{?}able warrior{\\?} across {WAR_PARTIES} {?WAR_PARTIES.PLURAL_FORM}war parties{?}war party{\\?}";
        private const string NoFiefsDescription = "{=RzO5SBUgJ}no fiefs";
        private const string FiefsDescription = "{=IA3iGGsYf}{FIEFS} {?FIEFS.PLURAL_FORM}fiefs{?}fief{\\?} that they {?AT_WAR}will{?}won't be able to{\\?} bring with them";

        private const string RelationVeryFriendly = "{=x47mq54mF}very friendly";
        private const string RelationFriendly = "{=taaRCPcrR}friendly";
        private const string RelationNeutral = "{=CqcPzOwVt}neutral";
        private const string RelationAggravated = "{=JuvIOEmNh}aggravated";
        private const string RelationHostile = "{=U3BvXUXgJ}hostile";

        private const string ButtonWelcomeText = "{=ey1dxwam7}Welcome them";
        private const string ButtonTurnAway = "{=wV0pIOvEg}Turn them away";

        public static void AwaitPlayerDecision(Clan clan)
        {
            TextObject inquiryHeader = new TextObject(PlayerInquiryHeader);
            TextObject inquiryBody = new TextObject(PlayerInquiryBody);
            SetEntityProperties(inquiryBody, "JOINING_CLAN", clan);

            inquiryBody.SetTextVariable("TROOPS_DESCRIPTION", GetTroopsDesc(clan));
            inquiryBody.SetTextVariable("FIEFS_DESCRIPTION", GetFiefsDesc(clan));
            inquiryBody.SetTextVariable("RELATION", GetRelationDesc(clan));
            inquiryBody.SetTextVariable("NEW_LINE", "\n");

            InformationManager.ShowInquiry(new InquiryData(inquiryHeader.ToString(), inquiryBody.ToString(), true, true, ButtonWelcomeText.ToLocalizedString(), ButtonTurnAway.ToLocalizedString(), () => ApplyPlayerDecision(clan, true), () => ApplyPlayerDecision(clan, false)), true);
        }

        private static TextObject GetTroopsDesc(Clan clan)
        {
            TextObject troopsDesc = new TextObject(TroopsDescription);
            int troopsCount = 0;
            int partiesCount = 0;
            foreach (PartyComponent warPartyComponent in clan.WarPartyComponents)
            {
                troopsCount += warPartyComponent.MobileParty.MemberRoster.TotalHealthyCount;
                ++partiesCount;
            }
            SetNumericVariable(troopsDesc, "TROOPS", troopsCount);
            SetNumericVariable(troopsDesc, "WAR_PARTIES", partiesCount);
            return troopsDesc;
        }

        private static TextObject GetFiefsDesc(Clan clan)
        {
            int fiefCount = clan.Fiefs.Count;
            if (fiefCount > 0)
            {
                TextObject fiefsDesc = new TextObject(FiefsDescription);
                SetNumericVariable(fiefsDesc, "FIEFS", clan.Fiefs.Count);
                fiefsDesc.SetTextVariable("AT_WAR", (clan.Kingdom != null && (clan.Kingdom.IsAtWarWith(Clan.PlayerClan.Kingdom) || clan.IsRulingClan())) ? 1 : 0);
                return fiefsDesc;
            }
            else
            {
                return new TextObject(NoFiefsDescription);
            }
        }

        private static TextObject? GetRelationDesc(Clan clan)
        {
            int relation = clan.Leader.GetRelation(Hero.MainHero);
            TextObject? relationText = new();
            if (relation > -10 && relation < 10)
            {
                relationText = new TextObject(RelationNeutral);
            }
            else
            if (relation >= 10 && relation < 50)
            {
                relationText = new TextObject(RelationFriendly);
            }
            else
            if (relation >= 50)
            {
                relationText = new TextObject(RelationVeryFriendly);
            }
            else
            if (relation > -50 && relation <= -10)
            {
                relationText = new TextObject(RelationAggravated);
            }
            else
            if (relation <= -50)
            {
                relationText = new TextObject(RelationHostile);
            }
            return relationText;
        }

        internal static void ApplyPlayerDecision(Clan clan, bool decisionIsWelcome)
        {
            AOEvents.Instance!.OnPlayerGotJoinRequest();

            if (!decisionIsWelcome)
            {
                return;
            }

            Kingdom targetKingdom = Clan.PlayerClan.Kingdom;
            Kingdom? currentKingdom = clan.Kingdom;
            bool isMerc = clan.IsUnderMercenaryService;
            bool rulerIsLeaving = SettingsHelper.SubSystemEnabled(SubSystemType.LeaderDefectionFix) && clan.IsRulingClan();

            try
            {
                if (currentKingdom != null)
                {
                    if ((rulerIsLeaving || currentKingdom.IsAtWarWith(targetKingdom)) && !isMerc)
                    {
                        ChangeKingdomAction.ApplyByLeaveWithRebellionAgainstKingdom(clan, targetKingdom, true);
                        if (rulerIsLeaving)
                        {
                            DestroyKingdomAction.Apply(currentKingdom);
                        }
                    }
                    else
                    {
                        ChangeKingdomAction.ApplyByLeaveKingdom(clan, false);
                    }
                }
            }
            catch (Exception ex)
            {
                MethodInfo? methodInfo = MethodBase.GetCurrentMethod() as MethodInfo;
                DebugHelper.HandleException(ex, methodInfo, "ApplyPlayerDecision (leave kingdom section)");
                return;
            }

            try
            {
                if ((currentKingdom != null && isMerc) || (currentKingdom is null && clan.IsMinorFaction))
                {
                    ChangeKingdomAction.ApplyByJoinFactionAsMercenary(clan, targetKingdom, TWHelpers.FactionHelper.GetMercenaryAwardFactorToJoinKingdom(clan, targetKingdom, false), true);
                }
                else
                {
                    ChangeKingdomAction.ApplyByJoinToKingdom(clan, targetKingdom, true);
                }
            }
            catch (Exception ex)
            {
                MethodInfo? methodInfo = MethodBase.GetCurrentMethod() as MethodInfo;
                DebugHelper.HandleException(ex, methodInfo, "ApplyPlayerDecision (join kingdom section)");
            }
        }
    }
}
