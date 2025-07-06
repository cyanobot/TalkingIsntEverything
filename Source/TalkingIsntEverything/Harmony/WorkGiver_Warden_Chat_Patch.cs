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
    [HarmonyPatch(typeof(WorkGiver_Warden_Chat), nameof(WorkGiver_Warden_Chat.JobOnThing))]
    public static class WorkGiver_Warden_Chat_Patch
    {
        static MethodInfo m_CapableOf = AccessTools.Method(typeof(PawnCapacitiesHandler), nameof(PawnCapacitiesHandler.CapableOf));
        static FieldInfo f_Talking = AccessTools.Field(typeof(PawnCapacityDefOf), nameof(PawnCapacityDefOf.Talking));
        static MethodInfo m_CanRecruit = AccessTools.Method(typeof(WorkGiver_Warden_Chat_Patch), nameof(WorkGiver_Warden_Chat_Patch.CanRecruit));

        //parameters are to preserve stack order compared to the CapableOf method we're replacing
        //first parameter is the instance that would be loaded to call the non-static CapableOf
        //second parameter is the capacity parameter taken by CapableOf
        public static bool CanRecruit(PawnCapacitiesHandler handler, PawnCapacityDef capacity)
        {
            //Log.Message($"Calling CanRecruit, capacity: {capacity}, CapableOf: {handler.CapableOf(capacity)}" +
            //    $", allowRecruit: {allowRecruit}");
            return allowRecruit || handler.CapableOf(capacity);
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> ienum_instructions)
        {
            //Log.Message("Calling Transpiler for WorkGiver_Warden_Chat_Patch");
            List<CodeInstruction> instructions = ienum_instructions.ToList();
            for (int i = 0; i < instructions.Count; i++)
            {
                CodeInstruction cur = instructions[i];
                CodeInstruction prev = i > 0 ? instructions[i - 1] : null;

                if (cur.Calls(m_CapableOf) && prev.LoadsField(f_Talking))
                {
                    //Log.Message("Transpiler found insert point, inserting");
                    yield return new CodeInstruction(OpCodes.Call, m_CanRecruit);
                }
                else
                {
                    yield return cur;
                }
            }
        }
    }
}