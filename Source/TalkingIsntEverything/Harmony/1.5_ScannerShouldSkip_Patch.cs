using HarmonyLib;
using Verse;
using RimWorld;
using static TalkingIsntEverything.Util;

namespace TalkingIsntEverything
{
#if RW_1_5
    [HarmonyPatch(typeof(FloatMenuMakerMap),"ScannerShouldSkip")]
    public static class ScannerShouldSkip_Patch
    {
        public static void Postfix(ref bool __result, Pawn pawn, WorkGiver_Scanner scanner, Thing t)
        {
            //Log.Message("ScannerShouldSkip_Patch - pawn: " + pawn + ", scanner: " + scanner + ", t: " + t + ", result: " +  __result);
            if (__result) return;
            if (scanner is WorkGiver_Warden_Chat || scanner is WorkGiver_Warden_Convert)
            {
                //Log.Message("relevant scanner found");
                if (t is Pawn pawn2 && IsMute(pawn) && !pawn2.IsPrisonerOfColony)
                {
                    //Log.Message("returning true");
                    __result = true;
                }
            }
        }
    }
#endif
}