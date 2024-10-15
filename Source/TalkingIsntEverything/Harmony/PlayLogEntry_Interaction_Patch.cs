using System;
using System.Collections.Generic;
using HarmonyLib;
using Verse;
using RimWorld;
using System.Reflection;
using static TalkingIsntEverything.Util;

namespace TalkingIsntEverything
{
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
            DebugLog("PlayLogEntry_Interaction_Patch - intDef: " + intDef + ", initiator: " + initiator + ", IsMute: " + IsMute(initiator)
                + ", recipient: " + recipient + ", IsMute: " + IsMute(recipient));
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

}