using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;
using RimWorld;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using static TalkingIsntEverything.Settings;

namespace TalkingIsntEverything
{
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

}