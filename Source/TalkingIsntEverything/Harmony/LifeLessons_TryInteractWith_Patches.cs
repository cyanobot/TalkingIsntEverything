using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static TalkingIsntEverything.Main;
using static TalkingIsntEverything.Util;

namespace TalkingIsntEverything
{
    [HarmonyPatch(typeof(Pawn_InteractionsTracker), "TryInteractWith")]
    [HarmonyBefore(new string[] { "lifelessons" })]
    public static class LifeLessons_TryInteractWith_Patch_Before
    {
        public static bool cached_DisableSocialRequirements;

        public static bool Prepare()
        {
            return f_DisableSocialRequirements != null;
        }

        public static void Prefix(Pawn ___pawn)
        {
            cached_DisableSocialRequirements = (bool)f_DisableSocialRequirements.GetValue(null);

            if (IsMute(___pawn))
            {
                f_DisableSocialRequirements.SetValue(null,true);
            }
        }
    }


    [HarmonyPatch(typeof(Pawn_InteractionsTracker), "TryInteractWith")]
    [HarmonyAfter(new string[] { "lifelessons" })]
    public static class LifeLessons_TryInteractWith_Patch_After
    {
        public static bool Prepare()
        {
            return f_DisableSocialRequirements != null;
        }

        public static void Prefix(Pawn ___pawn)
        {
            f_DisableSocialRequirements.SetValue(null, LifeLessons_TryInteractWith_Patch_Before.cached_DisableSocialRequirements);
        }
    }
}
