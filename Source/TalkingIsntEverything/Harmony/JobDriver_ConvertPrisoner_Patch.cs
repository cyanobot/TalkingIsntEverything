using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using static TalkingIsntEverything.Settings;
using static TalkingIsntEverything.Util;

namespace TalkingIsntEverything
{
    [HarmonyPatch()]
    public static class JobDriver_ConvertPrisoner_Patch
    {
        static FieldInfo f_ChitChat = AccessTools.Field(typeof(InteractionDefOf), nameof(InteractionDefOf.Chitchat));
        static FieldInfo f_Pawn = AccessTools.Field(typeof(JobDriver), nameof(JobDriver.pawn));
        static MethodInfo m_GetInteractionDef = AccessTools.Method(typeof(JobDriver_ConvertPrisoner_Patch), nameof(JobDriver_ConvertPrisoner_Patch.GetInteractionDef));

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

        public static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (Type nestedType in typeof(JobDriver_ConvertPrisoner).GetNestedTypes(
                                                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                                        .ToList()
                                        .Where(t => t.Name.Contains("MakeNewToils")))
            {
                //Log.Message($"Found nestedType: {nestedType}");
                foreach (MemberInfo nestedMember in nestedType.FindMembers(MemberTypes.Method,
                                                BindingFlags.Instance | BindingFlags.NonPublic,
                                                null, null)
                                        .Where(t => t.Name == "MoveNext"))
                {
                    //Log.Message($"nestedMember: {nestedMember}, as MethodBase {nestedMember as MethodBase}");
                    yield return nestedMember as MethodBase;
                }
            }
        }

        public static InteractionDef GetInteractionDef(Pawn pawn)
        {
            //Log.Message($"Calling JobDriver_ConvertPrisoner_Patch.GetInteractionDef - " +
            //    $"pawn: {pawn}, allowConvert: {allowConvert}, IsMute: {IsMute(pawn)}");
            if (allowConvert && IsMute(pawn))
            {
                return IntDefOf.CYB_Mute_ConvertPrisoner;
            }
            else return InteractionDefOf.Chitchat;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> ienum_instructions)
        {

            //Log.Message("Calling Transpiler for JobDriver_ConvertPrisoner_Patch");
            List<CodeInstruction> instructions = ienum_instructions.ToList();
            for (int i = 0; i < instructions.Count; i++)
            {
                CodeInstruction cur = instructions[i];

                if (cur.LoadsField(f_ChitChat))
                //if (false)
                {
                    //Log.Message("Transpiler found insert point, inserting");

                    //first need to load the pawn onto the stack to pass as argument to GetInteractionDef

                    //ldloc.1
                    //gets a reference to the JobDriver_ConvertPrisoner from the compiler-generated subclass that handles the MakeNewToils loop
                    yield return new CodeInstruction(OpCodes.Ldloc_1);

                    //ldfld class Verse.Pawn Verse.AI.JobDriver::pawn
                    //gets the "pawn" field from the JobDriver instance
                    yield return new CodeInstruction(OpCodes.Ldfld, f_Pawn);

                    //then with pawn on stack, call GetInteractionDef
                    yield return new CodeInstruction(OpCodes.Call, m_GetInteractionDef);

                    //should end up with an interactionDef on the stack just as we would have by loading ChitChat
                }
                else
                {
                    yield return cur;
                }
            }
        }
    }
}
