using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace TalkingIsntEverything
{
    [DefOf]
    static class IntDefOf
    {
        //custom
        public static InteractionDef CYB_EmptyInteraction;
        public static InteractionDef CYB_Mute_ConvertPrisoner;

        //abilities (speeches)

        [MayRequireIdeology]
        public static AbilityDef LeaderSpeech;

        [MayRequireIdeology]
        public static AbilityDef Trial;

        [MayRequireIdeology]
        public static AbilityDef ConversionRitual;

        //workgivers

        public static WorkGiverDef Tame;
        public static WorkGiverDef Train;
        public static WorkGiverDef ChatWithPrisoner;

        [MayRequireIdeology]
        public static WorkGiverDef ConvertPrisoner;
    }
}
