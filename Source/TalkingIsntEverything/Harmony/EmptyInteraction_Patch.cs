using HarmonyLib;
using Verse;
using RimWorld;
using System.Reflection;

namespace TalkingIsntEverything
{
    //empty interaction does nothing but counts as having interacted
    [HarmonyPatch(typeof(Pawn_InteractionsTracker),nameof(Pawn_InteractionsTracker.TryInteractWith))]
    public static class EmptyInteraction_Patch
    {
        public static FieldInfo f_lastInteractionTime = AccessTools.Field(typeof(Pawn_InteractionsTracker), "lastInteractionTime");
        public static FieldInfo f_lastInteraction = AccessTools.Field(typeof(Pawn_InteractionsTracker), "lastInteraction");
        public static FieldInfo f_lastInteractionDef = AccessTools.Field(typeof(Pawn_InteractionsTracker), "lastInteractionDef");

        public static bool Prefix(ref bool __result, Pawn_InteractionsTracker __instance, Pawn recipient, InteractionDef intDef)
        {
            if (DebugSettings.alwaysSocialFight) return true;

            if (intDef == IntDefOf.CYB_EmptyInteraction)
            {
                f_lastInteractionTime.SetValue(__instance, Find.TickManager.TicksGame);
                f_lastInteraction.SetValue(__instance, intDef.defName);
                f_lastInteractionDef.SetValue(__instance, intDef);

                if (recipient.interactions != null)
                {
                    f_lastInteractionTime.SetValue(recipient.interactions, Find.TickManager.TicksGame);
                    f_lastInteraction.SetValue(recipient.interactions, intDef.defName);
                    f_lastInteractionDef.SetValue(recipient.interactions, intDef);
                }

                __result = true;
                return false;
            }

            return true;
        }
    }

}