using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;
using RimWorld;
using static TalkingIsntEverything.Settings;
using RimWorld.Planet;
using Verse.AI;
using UnityEngine;
using Verse.AI.Group;

namespace TalkingIsntEverything
{

    [HarmonyPatch(typeof(Building_CommsConsole), "GetFailureReason")]
    public static class CommsConsole_Patch
    {
        public static void Postfix(ref FloatMenuOption __result, Pawn myPawn)
        {
            if (__result == null) return;
            //Log.Message("CommsConsole_Patch.Postfix - __result: " + __result + ", Label: " + __result?.Label);
            string matchText = "CannotUseReason".Translate("IncapableOfCapacity".Translate(PawnCapacityDefOf.Talking.label, myPawn.Named("PAWN")));
            //Log.Message("matchText: " + matchText);
            if (allowComms && __result.Label == matchText)
            {
                __result = null;
            }
        }
    }

}