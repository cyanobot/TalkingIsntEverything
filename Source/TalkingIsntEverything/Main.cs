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
using static TalkingIsntEverything.Settings;
using static TalkingIsntEverything.Util;

namespace TalkingIsntEverything
{
    public class Main : Mod
    {
        public Main(ModContentPack mcp) : base(mcp)
        {
            GetSettings<Settings>();

            Settings.wayBetterRomanceLoaded = LoadedModManager.RunningModsListForReading.Any(x => x.Name == "Way Better Romance");
        }

        public override string SettingsCategory()
        {
            return "Talking Isn't Everything";
        }

        static public void ApplySettings()
        {
            //Log.Message("ApplySettings - allowAnimals: " + allowAnimals + ", allowRomance: " + allowRomance + ", allowCasual: " + allowCasual + ", allowSlavery: " + allowSlavery);
            foreach(InteractionSwapDef swap in DefDatabase<InteractionSwapDef>.AllDefsListForReading)
            {
                if (swap.setting == "allowAnimals") swap.enabled = allowAnimals;
                else if (swap.setting == "allowRomance") swap.enabled = allowRomance;
                else if (swap.setting == "allowCasual") swap.enabled = allowCasual;
                else if (swap.setting == "allowSlavery") swap.enabled = allowSlavery;
                //Log.Message(swap + ".enabled: " + swap.enabled);
            }

            CalculateAffectedInteractions();

            if (allowAnimals)
            {
                DefDatabase<WorkGiverDef>.GetNamed("Tame").requiredCapacities.Remove(PawnCapacityDefOf.Talking);
                DefDatabase<WorkGiverDef>.GetNamed("Train").requiredCapacities.Remove(PawnCapacityDefOf.Talking);

                SetTalkingFactor(StatDefOf.TameAnimalChance, 0f);
                SetTalkingFactor(StatDefOf.TrainAnimalChance, 0f);
            }
            else
            {
                DefDatabase<WorkGiverDef>.GetNamed("Tame").requiredCapacities.AddDistinct(PawnCapacityDefOf.Talking);
                DefDatabase<WorkGiverDef>.GetNamed("Train").requiredCapacities.AddDistinct(PawnCapacityDefOf.Talking);

                SetTalkingFactor(StatDefOf.TameAnimalChance, defaultTameTalkingWeight);
                SetTalkingFactor(StatDefOf.TrainAnimalChance, defaultTrainTalkingWeight);
            }
            //Log.Message("tame required capacities: " + DefDatabase<WorkGiverDef>.GetNamed("Tame").requiredCapacities.ToStringSafeEnumerable());

            if (allowRomance && !allowCasual)
            {
                baseEmptySelectionWeight = 1f;
            }
            else
            {
                baseEmptySelectionWeight = 0f;
            }
        }

        public override void DoSettingsWindowContents(Rect inRect) => Settings.DoSettingsWindowContents(inRect);
    }

}
