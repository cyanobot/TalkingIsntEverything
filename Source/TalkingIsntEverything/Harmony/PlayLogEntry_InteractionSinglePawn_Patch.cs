﻿using System;
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
    public static class PlayLogEntry_InteractionSinglePawn_Patch
    {
        public static MethodBase TargetMethod()
        {
            return AccessTools.Constructor(typeof(PlayLogEntry_InteractionSinglePawn), new Type[] { typeof(InteractionDef), typeof(Pawn), typeof(List<RulePackDef>) });
        }

        public static void Prefix(ref InteractionDef intDef, Pawn initiator)
        {
            DebugLog("PlayLogEntry_InteractionSinglePawn_Patch - intDef: " + intDef + ", initiator: " + initiator + ", IsMute: " + IsMute(initiator));
            if (IsMute(initiator))
            {
                intDef = ReplacementFor(intDef, initiator, true);
            }

        }
    }

}