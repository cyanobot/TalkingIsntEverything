using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace TalkingIsntEverything
{
    public static class Util
    {
        public static float baseEmptySelectionWeight = 0f;
        public static float defaultTameTalkingWeight = 0.9f;
        public static float defaultTrainTalkingWeight = 0.7f;

        public static Dictionary<InteractionDef,List<InteractionSwapDef>> cachedAffectedInteractions = new Dictionary<InteractionDef, List<InteractionSwapDef>>();

        public static List<InteractionSwapDef> AllInteractionSwaps => DefDatabase<InteractionSwapDef>.AllDefsListForReading;

        public static Dictionary<InteractionDef, List<InteractionSwapDef>> AffectedInteractions
        {
            get
            {
                if (cachedAffectedInteractions.NullOrEmpty())
                {
                    CalculateAffectedInteractions();
                }
                return cachedAffectedInteractions;
            }
        }

        public static void CalculateAffectedInteractions()
        {
            if (AllInteractionSwaps.NullOrEmpty())
            {
                Log.Error("[TalkingIsntEverything] Mod is loaded but no InteractionSwapDefs found.");
                cachedAffectedInteractions = new Dictionary<InteractionDef, List<InteractionSwapDef>>
                {
                    { null, null }
                };
                return;
            }
            if (cachedAffectedInteractions == null) cachedAffectedInteractions = new Dictionary<InteractionDef, List<InteractionSwapDef>>();
            cachedAffectedInteractions.Clear();
            foreach (InteractionSwapDef swap in AllInteractionSwaps)
            {
                if (!swap.enabled) continue;
                if (swap.original == null) continue;
                if (!cachedAffectedInteractions.Keys.Contains(swap.original))
                {
                    cachedAffectedInteractions.Add(swap.original, new List<InteractionSwapDef>());
                }
                cachedAffectedInteractions[swap.original].Add(swap);
            }
            //Log.Message("CalculateAffectedInterations - cachedAffectedInteractions: " + cachedAffectedInteractions.Keys.ToStringSafeEnumerable());
        }

        public static bool IsMute(Pawn pawn)
        {
            return !pawn.health?.capacities?.CapableOf(PawnCapacityDefOf.Talking) ?? false;
        }

        public static bool InteractionAffected(InteractionDef interaction)
        {
            return AffectedInteractions.ContainsKey(interaction);
        }

        /*
        public static bool InteractionAffected(InteractionDef interaction, InteractionRole role)
        {
            if (AffectedInteractions.ContainsKey(interaction))
            {
                return AffectedInteractions[interaction].Any(swap => swap.role == InteractionRole.Either || swap.role == role);
            }
            return false;
        }
        */

        public static InteractionDef ReplacementFor(InteractionDef def, Pawn pawn, bool isInitiator)
        {
            //Log.Message("ReplacementFor - def: " + def + ", pawn: " + pawn);
            if (!InteractionAffected(def)) return def;
            List<InteractionSwapDef> swaps = AffectedInteractions[def];
            //Log.Message("swaps: " + swaps.ToStringSafeEnumerable());
            if (swaps.NullOrEmpty()) return def;
            foreach (InteractionSwapDef swap in swaps)
            {
                if (swap.role == InteractionRole.Either
                    || (isInitiator 
                        ? swap.role == InteractionRole.Initiator
                        : swap.role == InteractionRole.Recipient))
                {
                    //Log.Message("Found replacement: " + swap.replacement);
                    return swap.replacement;
                }
            }
            return def;
        }

        public static void SetTalkingFactor(StatDef stat, float weight)
        {
            PawnCapacityFactor capacityFactor = stat.capacityFactors?.Find(f => f.capacity == PawnCapacityDefOf.Talking);
            capacityFactor.weight = weight;
        }
    }
}
