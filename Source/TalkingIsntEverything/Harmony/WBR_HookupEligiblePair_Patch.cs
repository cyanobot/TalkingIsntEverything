using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using HarmonyLib;
using Verse;
using RimWorld;
using static TalkingIsntEverything.Settings;
using static TalkingIsntEverything.Util;

namespace TalkingIsntEverything
{
    [HarmonyPatch]
    public static class WBR_HookupEligiblePair_Patch
    {
        public static bool Prepare()
        {
            return wayBetterRomanceLoaded;
        }

        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                AccessTools.TypeByName("BetterRomance.HookupUtility"),
                "HookupEligiblePair",
                new Type[] { typeof(Pawn), typeof(Pawn), typeof(bool) }
                );
        }

        public static void Postfix(ref AcceptanceReport __result, Pawn initiator)
        {
            if (__result != true) return;

            if (!IsMute(initiator)) return;

            if (!allowRomance)
            {
                __result = "IncapableOfCapacity".Translate(PawnCapacityDefOf.Talking.label, initiator.Named("PAWN"));
            }
        }
    }
}
