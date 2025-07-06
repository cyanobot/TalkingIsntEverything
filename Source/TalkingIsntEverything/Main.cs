using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public static Type t_LL_DebugToggles;
        public static FieldInfo f_DisableSocialRequirements;

        public Main(ModContentPack mcp) : base(mcp)
        {
            GetSettings<Settings>();

            Settings.wayBetterRomanceLoaded = LoadedModManager.RunningModsListForReading.Any(x => x.Name == "Way Better Romance");
            lifeLessonsLoaded = ModsConfig.IsActive("GhostData.lifelessons");

            t_LL_DebugToggles = lifeLessonsLoaded ? AccessTools.TypeByName("LifeLessons.DebugToggles") : null;
            f_DisableSocialRequirements = lifeLessonsLoaded ? AccessTools.Field(t_LL_DebugToggles, "DisableSocialRequirements") : null;
        }

        public override string SettingsCategory()
        {
            return "Talking Isn't Everything";
        }

        public override void DoSettingsWindowContents(Rect inRect) => Settings.DoSettingsWindowContents(inRect);
    }

}
