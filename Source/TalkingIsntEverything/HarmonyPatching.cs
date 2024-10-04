using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;
using RimWorld;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using static TalkingIsntEverything.Util;
using static TalkingIsntEverything.Settings;
using RimWorld.Planet;
using Verse.AI;
using UnityEngine;
using Verse.AI.Group;

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

    //interaction log entry replacementss
    [HarmonyPatch]
    public static class PlayLogEntry_Interaction_Patch
    {
        public static MethodBase TargetMethod()
        {
            return AccessTools.Constructor(typeof(PlayLogEntry_Interaction), new Type[] { typeof(InteractionDef), typeof(Pawn), typeof(Pawn), typeof(List<RulePackDef>) });
        }

        public static void Prefix(ref InteractionDef intDef, Pawn initiator, Pawn recipient)
        {
            //Log.Message("PlayLogEntry_Interaction_Patch - intDef: " + intDef + ", initiator: " + initiator + ", IsMute: " + IsMute(initiator)
            //    + ", recipient: " + recipient + ", IsMute: " + IsMute(recipient));
            if (IsMute(initiator))
            {
                intDef = ReplacementFor(intDef, initiator, true);
                return;
            }
            if (IsMute(recipient))
            {
                intDef = ReplacementFor(intDef, recipient, false);
            }

        }
    }

    //allow mute pawns to initiate interactions that have an initiator swap defined
    [HarmonyPatch(typeof(InteractionUtility), nameof(InteractionUtility.CanInitiateInteraction))]
    public static class CanInitiateInteraction_Patch
    {
        public static bool Prefix(ref bool __result, Pawn pawn, InteractionDef interactionDef)
        {
            //if (interactionDef == InteractionDefOf.ReduceWill || interactionDef == InteractionDefOf.EnslaveAttempt) Log.Message("CanInitiateInteraction Prefix - pawn: " + pawn + ", def: " + interactionDef);
            //don't mess with non-mute pawns or interactions that aren't modified
            if (pawn == null || !IsMute(pawn) || (interactionDef != null && interactionDef != IntDefOf.CYB_EmptyInteraction && !AffectedInteractions.ContainsKey(interactionDef))) return true;

            //Log.Message("reached vanilla checks");
            //do the same other checks as vanilla - if failed pass back to vanilla in case anyone else is patching the same thing
            if (pawn.interactions == null) return true;
            if (!pawn.Awake()) return true;
            if (pawn.IsBurning()) return true;
            if (pawn.IsMutant) return true;
            if (pawn.IsInteractionBlocked(interactionDef, isInitiator: true, isRandom: false)) return true;
            
            if (interactionDef == null)             //enables random interactions
            {
                //Log.Message("interactionDef null, returning " + (allowRomance || allowCasual));
                __result = (allowRomance || allowCasual);
                return false;
            }

            if (interactionDef == IntDefOf.CYB_EmptyInteraction)
            {
                __result = true;
                return false;
            }

            //Log.Message("passed vanilla checks, affectedinteractions: " + AffectedInteractions[interactionDef].ToStringSafeEnumerable());
            List<InteractionSwapDef> swaps = AffectedInteractions[interactionDef];
            if (swaps.NullOrEmpty()) return true;
            if (swaps.Any(s => s.role == InteractionRole.Either || s.role == InteractionRole.Initiator))
            {
               //Log.Message("found swap");
                __result = true;
                return false;
            }

            return true;
        }
    }

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

    //remove designation warning for animal taming for pawns being mute
    [HarmonyPatch]
    public static class TameUtility_ShowDesignationWarnings_Patch
    {
        static Type t_TameUtility = typeof(TameUtility);
        static MethodInfo m_CapableOf = AccessTools.Method(typeof(PawnCapacitiesHandler), nameof(PawnCapacitiesHandler.CapableOf));
        static FieldInfo f_Talking = AccessTools.Field(typeof(PawnCapacityDefOf), nameof(PawnCapacityDefOf.Talking));
        static MethodInfo m_CanAnimals = AccessTools.Method(typeof(TameUtility_ShowDesignationWarnings_Patch), nameof(TameUtility_ShowDesignationWarnings_Patch.CanAnimals));

        public static IEnumerable<MethodBase> TargetMethods()
        {
            Type[] nestedTypes = t_TameUtility.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            foreach (Type nestedType in nestedTypes)
            {
                //Log.Message("nestedType: " + nestedType
                //        + ", CompilerGeneratedAttributes: " + nestedType.GetCustomAttributes(typeof(CompilerGeneratedAttribute)).ToStringSafeEnumerable());
                if (!nestedType.GetCustomAttributes(typeof(CompilerGeneratedAttribute)).Any()) continue;
                MethodInfo[] methods = nestedType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                foreach (MethodInfo method in methods)
                {
                    //Log.Message("method: " + method
                    //    + ", CompilerGeneratedAttributes: " + method.GetCustomAttributes(typeof(CompilerGeneratedAttribute)).ToStringSafeEnumerable()
                    //    + ", GetParameters: " + method.GetParameters().ToStringSafeEnumerable());
                    if (method.GetParameters().Length == 1
                        && method.GetParameters()[0].ParameterType == typeof(Pawn))
                    {
                        yield return method;
                    }
                }
            }
        }

        //parameters are to preserve stack order compared to the CapableOf method we're replacing
        //first parameter is the instance that would be loaded to call the non-static CapableOf
        //second parameter is the capacity parameter taken by CapableOf
        public static bool CanAnimals(PawnCapacitiesHandler handler,PawnCapacityDef capacity)
        {
            return allowAnimals || handler.CapableOf(capacity);
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> ienum_instructions)
        {
            List<CodeInstruction> instructions = ienum_instructions.ToList();
            for (int i = 0; i < instructions.Count; i++)
            {
                CodeInstruction cur = instructions[i];
                CodeInstruction prev = i > 0 ? instructions[i - 1] : null;

                if (cur.Calls(m_CapableOf) && prev.LoadsField(f_Talking))
                {
                    yield return new CodeInstruction(OpCodes.Call, m_CanAnimals);
                }
                else
                {
                    yield return cur;
                }
            }
        }
        
    }

    [HarmonyPatch(typeof(WorkGiver_Warden_Enslave), nameof(WorkGiver_Warden_Enslave.JobOnThing))]
    public static class WorkGiver_Warden_Enslave_Patch
    {
        static MethodInfo m_CapableOf = AccessTools.Method(typeof(PawnCapacitiesHandler), nameof(PawnCapacitiesHandler.CapableOf));
        static FieldInfo f_Talking = AccessTools.Field(typeof(PawnCapacityDefOf), nameof(PawnCapacityDefOf.Talking));
        static MethodInfo m_CanSlavery = AccessTools.Method(typeof(WorkGiver_Warden_Enslave_Patch), nameof(WorkGiver_Warden_Enslave_Patch.CanSlavery));

        //parameters are to preserve stack order compared to the CapableOf method we're replacing
        //first parameter is the instance that would be loaded to call the non-static CapableOf
        //second parameter is the capacity parameter taken by CapableOf
        public static bool CanSlavery(PawnCapacitiesHandler handler, PawnCapacityDef capacity)
        {
            return allowSlavery || handler.CapableOf(capacity);
        }

        public static bool Prepare(MethodBase original)
        {
            if (original == null)
            {
                if (!ModsConfig.IdeologyActive)
                {
                    return false;
                }
            }
            return true;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> ienum_instructions)
        {
            //Log.Message("Calling Transpiler for WorkGiver_Warden_Enslave_Patch");
            List<CodeInstruction> instructions = ienum_instructions.ToList();
            for (int i = 0; i < instructions.Count; i++)
            {
                CodeInstruction cur = instructions[i];
                CodeInstruction prev = i > 0 ? instructions[i - 1] : null;

                if (cur.Calls(m_CapableOf) && prev.LoadsField(f_Talking))
                {
                    //Log.Message("Transpiler found insert point, inserting");
                    yield return new CodeInstruction(OpCodes.Call, m_CanSlavery);
                }
                else
                {
                    yield return cur;
                }
            }
        }
    }

    [HarmonyPatch(typeof(RelationsUtility), nameof(RelationsUtility.RomanceEligible))]
    public static class RomanceEligible_Patch
    {
        static MethodInfo m_CapableOf = AccessTools.Method(typeof(PawnCapacitiesHandler), nameof(PawnCapacitiesHandler.CapableOf));
        static FieldInfo f_Talking = AccessTools.Field(typeof(PawnCapacityDefOf), nameof(PawnCapacityDefOf.Talking));
        static MethodInfo m_CanRomance = AccessTools.Method(typeof(RomanceEligible_Patch), nameof(RomanceEligible_Patch.CanRomance));

        //parameters are to preserve stack order compared to the CapableOf method we're replacing
        //first parameter is the instance that would be loaded to call the non-static CapableOf
        //second parameter is the capacity parameter taken by CapableOf
        public static bool CanRomance(PawnCapacitiesHandler handler, PawnCapacityDef capacity)
        {
            return allowRomance || handler.CapableOf(capacity);
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> ienum_instructions)
        {
            //Log.Message("Calling Transpiler for RomanceEligible_Patch");
            List<CodeInstruction> instructions = ienum_instructions.ToList();
            for (int i = 0; i < instructions.Count; i++)
            {
                CodeInstruction cur = instructions[i];
                CodeInstruction prev = i > 0 ? instructions[i - 1] : null;

                if (cur.Calls(m_CapableOf) && prev.LoadsField(f_Talking))
                {
                    //Log.Message("Transpiler found insert point, inserting");
                    yield return new CodeInstruction(OpCodes.Call, m_CanRomance);
                }
                else
                {
                    yield return cur;
                }
            }
        }
    }

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