using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using static TalkingIsntEverything.Util;

namespace TalkingIsntEverything
{
    [StaticConstructorOnStartup]
    class Init
    {
        static Init()
        {
            defaultTameTalkingWeight = StatDefOf.TameAnimalChance.capacityFactors?.Find(f => f.capacity == PawnCapacityDefOf.Talking)?.weight ?? 0f;
            defaultTrainTalkingWeight = StatDefOf.TrainAnimalChance.capacityFactors?.Find(f => f.capacity == PawnCapacityDefOf.Talking)?.weight ?? 0f;

            Main.ApplySettings();

            var harmony = new Harmony("cyanobot.TalkingIsntEverything");

            harmony.PatchAll();
        }
    }
}
