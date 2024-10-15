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
using Verse.AI;

namespace TalkingIsntEverything
{
    [HarmonyPatch]
    class WBR_JoyGiver_Date_Patch
    {
        public static bool Prepare()
        {
            return wayBetterRomanceLoaded;
        }

        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                AccessTools.TypeByName("BetterRomance.JoyGiver_Date"),
                nameof(JoyGiver.TryGiveJob),
                new Type[] {typeof(Pawn)}
                );
        }

        public static void Postfix(ref Job __result, Pawn pawn)
        {
            if (__result == null) return;
            if (!IsMute(pawn)) return;
            JobDef jobDef = __result.def;

            if (jobDef.defName == "ProposeDate" && !allowRomance)
            {
                __result = null;
            }
            else if (jobDef.defName == "ProposeHangout" && !allowCasual)
            {
                __result = null;
            }
        }
    }
}
