using HarmonyLib;
using Verse;
using static TalkingIsntEverything.Util;

namespace TalkingIsntEverything
{
    //when defs are hot reloaded, reload our cached def info
    [HarmonyPatch(typeof(PlayDataLoader), "ResetStaticDataPre")]
    public static class ResetStaticDataPre_Patch
    {
        public static void Postfix()
        {
            CalculateAffectedInteractions();
        }
    }

}