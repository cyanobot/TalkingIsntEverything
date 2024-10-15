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
    class WBR_HookupEligible_Patch
    {
        public static bool Prepare()
        {
            return wayBetterRomanceLoaded;
        }

        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                AccessTools.TypeByName("BetterRomance.HookupUtility"),
                "HookupEligible",
                new Type[] { typeof(Pawn) , typeof(bool) }
                );
        }

        public static void Postfix(ref AcceptanceReport __result, Pawn pawn, bool initiator)
        {
            if (!__result.Accepted) return;
            if (!initiator) return;

            if (!IsMute(pawn)) return;

            if (!allowRomance)
            {
                __result = "IncapableOfCapacity".Translate(PawnCapacityDefOf.Talking.label, pawn.Named("PAWN"));
            }
        }
    }
}
