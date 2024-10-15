using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RimWorld;
using Verse;
using HarmonyLib;

namespace TalkingIsntEverything
{
    public class InteractionSwapDef : Def
    {
        public string setting = "";
        public InteractionRole role;
        public InteractionDef original;
        public InteractionDef replacement;

        public bool enabled = true;

    }
}
