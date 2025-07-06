using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;
using RimWorld;
using System.Reflection;
using System.Reflection.Emit;
using static TalkingIsntEverything.Settings;
using Verse.AI;

namespace TalkingIsntEverything
{
    [HarmonyPatch(typeof(SchoolUtility),nameof(SchoolUtility.CanTeachNow))]
    public static class CanTeachNow_Patch
    {
        static MethodInfo m_CapableOf = AccessTools.Method(typeof(PawnCapacitiesHandler), nameof(PawnCapacitiesHandler.CapableOf));
        static FieldInfo f_Talking = AccessTools.Field(typeof(PawnCapacityDefOf), nameof(PawnCapacityDefOf.Talking));
        static MethodInfo m_CanTeach = AccessTools.Method(typeof(CanTeachNow_Patch), nameof(CanTeachNow_Patch.CanTeach));

        static FieldInfo f_Pawn = AccessTools.Field(typeof(PawnCapacitiesHandler), "pawn");

        //parameters are to preserve stack order compared to the CapableOf method we're replacing
        //first parameter is the instance that would be loaded to call the non-static CapableOf
        //second parameter is the capacity parameter taken by CapableOf
        public static bool CanTeach(PawnCapacitiesHandler handler, PawnCapacityDef capacity)
        {
            Log.Message($"CanTeachNow - pawn: {f_Pawn.GetValue(handler)}, capacity: {capacity}" +
                $", allowTeach: {allowTeach}");
            return allowTeach || handler.CapableOf(capacity);
        }


        public static bool Prepare(MethodBase original)
        {
            if (original == null)
            {
                if (!ModsConfig.BiotechActive)
                {
                    return false;
                }
            }
            return true;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> ienum_instructions)
        {
            //Log.Message("Calling Transpiler for CanTeachNow_Patch");
            List<CodeInstruction> instructions = ienum_instructions.ToList();
            for (int i = 0; i < instructions.Count; i++)
            {
                CodeInstruction cur = instructions[i];
                CodeInstruction prev = i > 0 ? instructions[i - 1] : null;

                if (cur.Calls(m_CapableOf) && prev.LoadsField(f_Talking))
                {
                    //Log.Message("Transpiler found insert point, inserting");
                    yield return new CodeInstruction(OpCodes.Call, m_CanTeach);

                }
                else
                {
                    yield return cur;
                }
            }
        }
    }
    
    /*
    [HarmonyPatch(typeof(WorkGiver_Teach), nameof(WorkGiver_Teach.ShouldSkip))]
    public static class Test_WorkGiver_Teach_ShouldSkip_Patch
    {
        public static void Postfix(Pawn pawn, bool __result)
        {
            Log.Message($"WorkGiver_Teach.ShouldSkip - pawn: {pawn}, result: {__result}");
        }
    }


    [HarmonyPatch(typeof(WorkGiver_Teach), nameof(WorkGiver_Teach.HasJobOnThing))]
    public static class Test_WorkGiver_Teach_HasJobOnThing_Patch
    {
        public static void Postfix(Pawn pawn, Thing t, bool __result)
        {
            Log.Message($"WorkGiver_Teach.HasJobOnThing - pawn: {pawn}, thing: {t}, result: {__result}");
        }
    }


    [HarmonyPatch(typeof(WorkGiver_Teach), nameof(WorkGiver_Teach.JobOnThing))]
    public static class Test_WorkGiver_Teach_JobOnThing_Patch
    {
        public static void Postfix(Pawn pawn, Thing t, Job __result)
        {
            Log.Message($"WorkGiver_Teach.JobOnThing - pawn: {pawn}, thing: {t}, result: {__result}");
        }
    }


    [HarmonyPatch(typeof(SchoolUtility), nameof(SchoolUtility.FindTeacher))]
    public static class Test_SchoolUtility_FindTeacher_Patch
    {
        public static void Postfix(Pawn child, Pawn __result)
        {
            Log.Message($"SchoolUtility.FindTeacher - pawn: {child}, result: {__result}");
        }
    }


    [HarmonyPatch(typeof(LearningGiver_Lessontaking), nameof(LearningGiver_Lessontaking.CanDo))]
    public static class Test_LearningGiver_Lessontaking_CanDo_Patch
    {
        public static void Postfix(Pawn pawn, bool __result)
        {
            Log.Message($"LearningGiver_Lessontaking.CanDo - pawn: {pawn}, result: {__result}");
        }
    }
    */
}
