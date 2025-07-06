using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;
using RimWorld;
using System.Reflection;
using System.Reflection.Emit;
using static TalkingIsntEverything.Settings;

namespace TalkingIsntEverything
{
    [HarmonyPatch(typeof(CompAbility_RequiresCapacity),nameof(CompAbility_RequiresCapacity.GizmoDisabled))]
    public static class CompAbility_RequiresCapacity_GizmoDisabled_Patch
    {
        public static bool Postfix(bool __result, CompAbility_RequiresCapacity __instance)
        {
            //if gizmo already not disabled, don't interfere
            if (!__result) return __result;

            //only interfere if talking is the capacity checked
            if (__instance.Props.capacity == PawnCapacityDefOf.Talking)
            {
                if (allowSpeech)
                {
                    //throne speech from Royalty
                    if (ModsConfig.RoyaltyActive && __instance.parent.def == AbilityDefOf.Speech)
                    {
                        //don't disable
                        return false;
                    }

                    //leader speech from Ideology
                    if (ModsConfig.IdeologyActive && __instance.parent.def == IntDefOf.LeaderSpeech)
                    {
                        //don't disable
                        return false;
                    }
                }

                if (allowConvert)
                {
                    //conversion ritual from Ideology
                    if (ModsConfig.IdeologyActive && __instance.parent.def == IntDefOf.ConversionRitual)
                    {
                        //don't disable
                        return false;
                    }
                }

                if (allowRoles)
                {
                    //trial from Ideology
                    if (ModsConfig.IdeologyActive && __instance.parent.def == IntDefOf.Trial)
                    {
                        //don't disable
                        return false;
                    }
                }
            }

            //all other cases don't interfere
            return __result;
        }
    }
}
