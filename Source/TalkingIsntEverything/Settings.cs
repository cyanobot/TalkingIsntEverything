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

namespace TalkingIsntEverything
{
    public class Settings : ModSettings
    {
        public static bool wayBetterRomanceLoaded = false;

        public static bool allowAnimals = true;
        public static bool allowRomance = true;
        public static bool allowCasual = false;
        public static bool allowSlavery = false;
        public static bool allowComms = false;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref allowAnimals, "allowAnimals", allowAnimals, true);
            Scribe_Values.Look(ref allowRomance, "allowRomance", allowRomance, true);
            Scribe_Values.Look(ref allowCasual, "allowCasual", allowCasual, true);
            Scribe_Values.Look(ref allowSlavery, "allowSlavery", allowSlavery, true);
            Scribe_Values.Look(ref allowComms, "allowComms", allowComms, true);
        }
        public static void DoSettingsWindowContents(Rect rect)
        {
            float margin_betweenSettings = 10f;
            float margin_checkBoxToLabel = 5f;

            float curY = rect.y + margin_betweenSettings;
            float curX = rect.x;

            //allow animals

            Widgets.Checkbox(new Vector2(curX, curY), ref allowAnimals);
            curX += Widgets.CheckboxSize + margin_checkBoxToLabel;

            string text_AllowAnimals = "CYB_SettingLabel_AllowAnimals".Translate();
            string tooltip_AllowAnimals = "CYB_SettingDesc_AllowAnimals".Translate();

            Vector2 size_label_allowAnimals = Text.CalcSize(text_AllowAnimals);
            curY += (Widgets.CheckboxSize - size_label_allowAnimals.y) / 2;
            Widgets.Label(curX, ref curY, size_label_allowAnimals.x + 2f, text_AllowAnimals, tip: new TipSignal(tooltip_AllowAnimals));

            curX = rect.x;
            curY += margin_betweenSettings;

            //allow romance

            Widgets.Checkbox(new Vector2(curX, curY), ref allowRomance);
            curX += Widgets.CheckboxSize + margin_checkBoxToLabel;

            string text_AllowRomance = "CYB_SettingLabel_AllowRomance".Translate();
            string tooltip_AllowRomance = "CYB_SettingDesc_AllowRomance".Translate();

            Vector2 size_label_allowRomance = Text.CalcSize(text_AllowRomance);
            curY += (Widgets.CheckboxSize - size_label_allowRomance.y) / 2;
            Widgets.Label(curX, ref curY, size_label_allowRomance.x + 2f, text_AllowRomance, tip: new TipSignal(tooltip_AllowRomance));

            curX = rect.x;
            curY += margin_betweenSettings;

            //allow casual

            Widgets.Checkbox(new Vector2(curX, curY), ref allowCasual);
            curX += Widgets.CheckboxSize + margin_checkBoxToLabel;

            string text_AllowCasual = "CYB_SettingLabel_AllowCasual".Translate();
            string tooltip_AllowCasual = "CYB_SettingDesc_AllowCasual".Translate();

            Vector2 size_label_allowCasual = Text.CalcSize(text_AllowCasual);
            curY += (Widgets.CheckboxSize - size_label_allowCasual.y) / 2;
            Widgets.Label(curX, ref curY, size_label_allowCasual.x + 2f, text_AllowCasual, tip: new TipSignal(tooltip_AllowCasual));

            curX = rect.x;
            curY += margin_betweenSettings;

            //allow slavery

            Widgets.Checkbox(new Vector2(curX, curY), ref allowSlavery, disabled: !ModsConfig.IdeologyActive);
            curX += Widgets.CheckboxSize + margin_checkBoxToLabel;

            string text_AllowSlavery = "CYB_SettingLabel_AllowSlavery".Translate() + (ModsConfig.IdeologyActive ? (TaggedString)"" : " (" + "CYB_IdeologyRequired".Translate() + ")");
            string tooltip_AllowSlavery = "CYB_SettingDesc_AllowSlavery".Translate();

            Vector2 size_label_allowSlavery = Text.CalcSize(text_AllowSlavery);
            curY += (Widgets.CheckboxSize - size_label_allowSlavery.y) / 2;
            if (!ModsConfig.IdeologyActive) GUI.color = new Color(1f, 1f, 1f, 0.2f);
            Widgets.Label(curX, ref curY, size_label_allowSlavery.x + 2f, text_AllowSlavery, tip: new TipSignal(tooltip_AllowSlavery));
            GUI.color = Color.white;

            curX = rect.x;
            curY += margin_betweenSettings;

            //allow comms

            Widgets.Checkbox(new Vector2(curX, curY), ref allowComms);
            curX += Widgets.CheckboxSize + margin_checkBoxToLabel;

            string text_AllowComms = "CYB_SettingLabel_AllowComms".Translate();
            string tooltip_AllowComms = "CYB_SettingDesc_AllowComms".Translate();

            Vector2 size_label_allowComms = Text.CalcSize(text_AllowComms);
            curY += (Widgets.CheckboxSize - size_label_allowComms.y) / 2;
            Widgets.Label(curX, ref curY, size_label_allowComms.x + 2f, text_AllowComms, tip: new TipSignal(tooltip_AllowComms));

            curX = rect.x;
            curY += margin_betweenSettings;

            //end

            Main.ApplySettings();
        }

        /*
        public static void DoSettingsWindowContents(Rect rect)
        {
            Listing_Standard l = new Listing_Standard(GameFont.Small)
            {
                ColumnWidth = rect.width
            };

            l.Begin(rect);

            l.CheckboxLabeled("CYB_SettingLabel_AllowAnimals".Translate(), ref allowAnimals, "CYB_SettingDesc_AllowAnimals".Translate());
            l.CheckboxLabeled("CYB_SettingLabel_AllowRomance".Translate(), ref allowRomance, "CYB_SettingDesc_AllowRomance".Translate());
            l.CheckboxLabeled("CYB_SettingLabel_AllowCasual".Translate(), ref allowCasual, "CYB_SettingDesc_AllowCasual".Translate());
            l.CheckboxLabeled("CYB_SettingLabel_AllowSlavery".Translate(), ref allowSlavery, "CYB_SettingDesc_AllowSlavery".Translate());

            l.End();

            Main.ApplySettings();
        }
        */
    }
}
