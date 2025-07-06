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
    public class Settings : ModSettings
    {
        public const float margin_betweenSettings = 10f;
        public const float margin_checkBoxToLabel = 5f;
        public static Color greyoutColor = new Color(1f, 1f, 1f, 0.2f);

        public static bool wayBetterRomanceLoaded = false;
        public static bool lifeLessonsLoaded = false;

        public static bool allowAnimals = true;
        public static bool allowRomance = true;
        public static bool allowCasual = false;
        public static bool allowSlavery = true;
        public static bool allowRecruit = false;
        public static bool allowComms = true;
        public static bool allowConvert = false;
        public static bool allowTrial = true;
        public static bool allowSpeech = false;
        public static bool allowTeach = true;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref allowAnimals, "allowAnimals", allowAnimals, true);
            Scribe_Values.Look(ref allowRomance, "allowRomance", allowRomance, true);
            Scribe_Values.Look(ref allowCasual, "allowCasual", allowCasual, true);
            Scribe_Values.Look(ref allowSlavery, "allowSlavery", allowSlavery, true);
            Scribe_Values.Look(ref allowRecruit, "allowRecruit", allowRecruit, true);
            Scribe_Values.Look(ref allowComms, "allowComms", allowComms, true);
            Scribe_Values.Look(ref allowConvert, "allowConvert", allowConvert, true);
            Scribe_Values.Look(ref allowTrial, "allowTrial", allowTrial, true);
            Scribe_Values.Look(ref allowSpeech, "allowSpeech", allowSpeech, true);
            Scribe_Values.Look(ref allowTeach, "allowTeach", allowTeach, true);
        }
        public static void DoSettingsWindowContents(Rect rect)
        {
            float curY = rect.y + margin_betweenSettings;
            float curX = rect.x;

            SettingEntry(rect, ref curX, ref curY, ref allowAnimals, "AllowAnimals");

            SettingEntry(rect, ref curX, ref curY, ref allowRomance, "AllowRomance");

            SettingEntry(rect, ref curX, ref curY, ref allowCasual, "AllowCasual");

            SettingEntry(rect, ref curX, ref curY, ref allowComms, "AllowComms");

            SettingEntry(rect, ref curX, ref curY, ref allowRecruit, "AllowRecruit");

            SettingEntry(rect, ref curX, ref curY, ref allowSlavery, "AllowSlavery", () => ModsConfig.IdeologyActive, "CYB_IdeologyRequired");

            SettingEntry(rect, ref curX, ref curY, ref allowConvert, "AllowConvert", () => ModsConfig.IdeologyActive, "CYB_IdeologyRequired");

            SettingEntry(rect, ref curX, ref curY, ref allowTrial, "AllowTrial", () => ModsConfig.IdeologyActive, "CYB_IdeologyRequired");

            SettingEntry(rect, ref curX, ref curY, ref allowSpeech, "AllowSpeech");

            SettingEntry(rect, ref curX, ref curY, ref allowTeach, "AllowTeach", () => ModsConfig.BiotechActive, "CYB_BiotechRequired");

            //end

            ApplySettings();
        }

        public static void SettingEntry(Rect rect, ref float curX, ref float curY, ref bool settingBool, string translateKey, Func<bool> enableCondition = null, string disableReasonKey = "Disabled")
        {
            Widgets.Checkbox(new Vector2(curX, curY), ref settingBool);
            curX += Widgets.CheckboxSize + margin_checkBoxToLabel;

            string text = ("CYB_SettingLabel_" + translateKey).Translate();
            if (enableCondition != null && !enableCondition())
            {
                text += " (" + disableReasonKey.Translate() + ")";
            }
            string tooltip = ("CYB_SettingDesc_" + translateKey).Translate();

            Vector2 size_label = Text.CalcSize(text);
            curY += (Widgets.CheckboxSize - size_label.y) / 2;
            if (enableCondition != null && !enableCondition()) GUI.color = greyoutColor;
            Widgets.Label(curX, ref curY, size_label.x + 2f, text, tip: new TipSignal(tooltip));
            GUI.color = Color.white;

            curX = rect.x;
            curY += margin_betweenSettings;
        }
        static public void ApplySettings()
        {
            //Log.Message("ApplySettings - allowAnimals: " + allowAnimals + ", allowRomance: " + allowRomance + ", allowCasual: " + allowCasual + ", allowSlavery: " + allowSlavery);
            foreach (InteractionSwapDef swap in DefDatabase<InteractionSwapDef>.AllDefsListForReading)
            {
                if (swap.setting == "allowAnimals") swap.enabled = allowAnimals;
                else if (swap.setting == "allowRomance") swap.enabled = allowRomance;
                else if (swap.setting == "allowCasual") swap.enabled = allowCasual;
                else if (swap.setting == "allowSlavery") swap.enabled = allowSlavery;
                else if (swap.setting == "allowRecruit") swap.enabled = allowRecruit;
                else if (swap.setting == "allowConvert") swap.enabled = allowConvert;
                else if (swap.setting == "allowTrial") swap.enabled = allowTrial;
                else if (swap.setting == "allowTeach") swap.enabled = allowTeach;

                //Log.Message(swap + ".enabled: " + swap.enabled);
            }

            CalculateAffectedInteractions();

            if (allowAnimals)
            {
                IntDefOf.Tame.requiredCapacities.Remove(PawnCapacityDefOf.Talking);
                IntDefOf.Train.requiredCapacities.Remove(PawnCapacityDefOf.Talking);

                SetTalkingFactor(StatDefOf.TameAnimalChance, 0f);
                SetTalkingFactor(StatDefOf.TrainAnimalChance, 0f);
            }
            else
            {
                IntDefOf.Tame.requiredCapacities.AddDistinct(PawnCapacityDefOf.Talking);
                IntDefOf.Train.requiredCapacities.AddDistinct(PawnCapacityDefOf.Talking);

                SetTalkingFactor(StatDefOf.TameAnimalChance, defaultTameTalkingWeight);
                SetTalkingFactor(StatDefOf.TrainAnimalChance, defaultTrainTalkingWeight);
            }
            //Log.Message("tame required capacities: " + DefDatabase<WorkGiverDef>.GetNamed("Tame").requiredCapacities.ToStringSafeEnumerable());

            if (allowRecruit)
            {
                IntDefOf.ChatWithPrisoner.requiredCapacities.Remove(PawnCapacityDefOf.Talking);
            }
            else
            {
                IntDefOf.ChatWithPrisoner.requiredCapacities.AddDistinct(PawnCapacityDefOf.Talking);
            }

            //Log.Message("ChatWithPrisoner required capacities: " + IntDefOf.ChatWithPrisoner.requiredCapacities.ToStringSafeEnumerable());

            if (ModsConfig.IdeologyActive)
            {
                if (allowConvert)
                {
                    IntDefOf.ConvertPrisoner.requiredCapacities.Remove(PawnCapacityDefOf.Talking);
                }
                else
                {
                    IntDefOf.ConvertPrisoner.requiredCapacities.AddDistinct(PawnCapacityDefOf.Talking);
                }
            }

            if (ModsConfig.BiotechActive)
            {
                if (allowTeach)
                {
                    IntDefOf.ChildcarerTeach.requiredCapacities.Remove(PawnCapacityDefOf.Talking);
                }
                else
                {
                    IntDefOf.ChildcarerTeach.requiredCapacities.AddDistinct(PawnCapacityDefOf.Talking);
                }
            }
            //Log.Message("ChildcarerTeach required capacities: " + IntDefOf.ChildcarerTeach.requiredCapacities.ToStringSafeEnumerable());

            if (allowRomance && !allowCasual)
            {
                baseEmptySelectionWeight = 1f;
            }
            else
            {
                baseEmptySelectionWeight = 0f;
            }

        }
    }
}
