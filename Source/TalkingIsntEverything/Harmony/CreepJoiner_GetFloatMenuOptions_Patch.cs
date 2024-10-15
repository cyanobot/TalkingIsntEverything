using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using HarmonyLib;
using Verse;
using RimWorld;
using static TalkingIsntEverything.Util;
using Verse.AI;

namespace TalkingIsntEverything
{
    [HarmonyPatch(typeof(Pawn_CreepJoinerTracker),nameof(Pawn_CreepJoinerTracker.GetFloatMenuOptions))]
    public static class CreepJoiner_GetFloatMenuOptions_Patch
    {
        public static void Postfix(ref IEnumerable<FloatMenuOption> __result, Pawn_CreepJoinerTracker __instance, Pawn selPawn)
        {
            //don't mess with non-mute pawns
            if (!IsMute(selPawn)) return;

            List<FloatMenuOption> opts_in = __result.ToList();
            List<FloatMenuOption> opts_out = new List<FloatMenuOption>();

            foreach (FloatMenuOption option in opts_in)
            {
                if (option.Label == "CannotTalkTo".Translate(__instance.Pawn) + ": " + "Incapable".Translate().CapitalizeFirst())
                {
                    opts_out.Add(
                        FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("TalkTo".Translate(__instance.Pawn),
                        delegate
                        {
                            Job job = JobMaker.MakeJob(JobDefOf.TalkCreepJoiner, __instance.Pawn);
                            job.playerForced = true;
                            selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                        }), 
                        selPawn, __instance.Pawn)
                        );
                }
                else
                {
                    opts_out.Add(option);
                }
            }

            __result = opts_out;
        }
    }
}
