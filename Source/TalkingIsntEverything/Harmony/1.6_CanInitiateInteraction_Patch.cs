using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;
using RimWorld;
using static TalkingIsntEverything.Util;
using static TalkingIsntEverything.Settings;
using System.Reflection;

namespace TalkingIsntEverything
{
#if RW_1_6
    //allow mute pawns to initiate interactions that have an initiator swap defined
    [HarmonyPatch(typeof(SocialInteractionUtility), nameof(SocialInteractionUtility.CanInitiateInteraction))]
    public static class CanInitiateInteraction_Patch
    {
        /*
        public static void Prepare(MethodBase original)
        {
            Log.Message($"New_CanInitiateInteraction_Patch - original: {original}" +
                $", SocialInteractionUtility type: {typeof(SocialInteractionUtility)}" +
                $", CanInitiateInteraction: {typeof(SocialInteractionUtility).GetMethod(nameof(SocialInteractionUtility.CanInitiateInteraction))}");
        }
        */

        public static bool Prefix(ref bool __result, Pawn pawn, InteractionDef interactionDef)
        {
            //if (interactionDef == InteractionDefOf.ReduceWill || interactionDef == InteractionDefOf.EnslaveAttempt)
            //if (interactionDef != null && pawn.RaceProps.Humanlike) DebugLog("CanInitiateInteraction Prefix - pawn: " + pawn + ", def: " + interactionDef);
            //don't mess with non-mute pawns or interactions that aren't modified
            if (pawn == null || !IsMute(pawn) || !pawn.RaceProps.Humanlike || (interactionDef != null && interactionDef != IntDefOf.CYB_EmptyInteraction && !AffectedInteractions.ContainsKey(interactionDef))) return true;

            //if (interactionDef != null) DebugLog("reached vanilla checks");
            //do the same other checks as vanilla - if failed pass back to vanilla in case anyone else is patching the same thing
            if (pawn.interactions == null) return true;
            if (!pawn.Awake()) return true;
            if (pawn.IsBurning()) return true;
            if (pawn.IsMutant) return true;
            if (pawn.IsInteractionBlocked(interactionDef, isInitiator: true, isRandom: false)) return true;
            
            if (interactionDef == null)             //enables random interactions
            {
                //DebugLog("interactionDef null, returning " + (allowRomance || allowCasual));
                __result = (allowRomance || allowCasual);
                return false;
            }

            if (interactionDef == IntDefOf.CYB_EmptyInteraction)
            {
                __result = true;
                return false;
            }

            //DebugLog("passed vanilla checks, affectedinteractions[interactionDef]: " + AffectedInteractions[interactionDef].ToStringSafeEnumerable());
            List<InteractionSwapDef> swaps = AffectedInteractions[interactionDef];
            if (swaps.NullOrEmpty()) return true;                                       //if found no swaps, return to vanilla
            if (swaps.Any(s => s.role == InteractionRole.Either || s.role == InteractionRole.Initiator))
            {
                //DebugLog("found swap");
                __result = true;
                return false;
            }

            return true;
        }

        /*
        public static void Postfix(bool __result, Pawn pawn, InteractionDef interactionDef)
        {
            if (interactionDef != null && pawn.RaceProps.Humanlike) DebugLog("CanInitiateInteraction Postfix - pawn: " + pawn + ", interactionDef: " + interactionDef + ", result: " + __result);
        }
        */
    }
#endif
}