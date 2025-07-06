using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;
using RimWorld;
using System.Reflection;
using System.Reflection.Emit;
using static TalkingIsntEverything.Settings;

namespace TalkingIsntEverything
{
    [HarmonyPatch(typeof(WorkGiver_Warden_Convert), nameof(WorkGiver_Warden_Convert.JobOnThing))]
    public static class WorkGiver_Warden_Convert_Patch
    {
        static MethodInfo m_CapableOf = AccessTools.Method(typeof(PawnCapacitiesHandler), nameof(PawnCapacitiesHandler.CapableOf));
        static FieldInfo f_Talking = AccessTools.Field(typeof(PawnCapacityDefOf), nameof(PawnCapacityDefOf.Talking));
        static MethodInfo m_CanConvert = AccessTools.Method(typeof(WorkGiver_Warden_Convert_Patch), nameof(WorkGiver_Warden_Convert_Patch.CanConvert));

        //parameters are to preserve stack order compared to the CapableOf method we're replacing
        //first parameter is the instance that would be loaded to call the non-static CapableOf
        //second parameter is the capacity parameter taken by CapableOf
        public static bool CanConvert(PawnCapacitiesHandler handler, PawnCapacityDef capacity)
        {
            return allowConvert || handler.CapableOf(capacity);
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
            //Log.Message("Calling Transpiler for WorkGiver_Warden_Convert_Patch");
            List<CodeInstruction> instructions = ienum_instructions.ToList();
            for (int i = 0; i < instructions.Count; i++)
            {
                CodeInstruction cur = instructions[i];
                CodeInstruction prev = i > 0 ? instructions[i - 1] : null;

                if (cur.Calls(m_CapableOf) && prev.LoadsField(f_Talking))
                {
                    //Log.Message("Transpiler found insert point, inserting");
                    yield return new CodeInstruction(OpCodes.Call, m_CanConvert);
                }
                else
                {
                    yield return cur;
                }
            }
        }
    }

}