using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using static TalkingIsntEverything.Util;

namespace TalkingIsntEverything
{

	public class InteractionWorker_Empty : InteractionWorker
	{
		public static float weight_Chitchat = 1f;
		public static float baseWeight_DeepTalk = 0.075f;
		public static float baseWeight_Slight = 0.02f;
		public static float baseWeight_Insult = 0.007f;

		private static readonly SimpleCurve DeepTalkCompatibilityFactorCurve = new SimpleCurve
	{
		new CurvePoint(-1.5f, 0f),
		new CurvePoint(-0.5f, 0.1f),
		new CurvePoint(0.5f, 1f),
		new CurvePoint(1f, 1.8f),
		new CurvePoint(2f, 3f)
	};

		public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
		{
			//Log.Message("Calling InteractionWorker_Empty.RandomSelectionWeight - initiator: " + initiator + ", recipient: " + recipient);

			if (!IsMute(initiator)) return 0f;

			float weight = baseEmptySelectionWeight;
			if (weight == 0f) return 0f;

			float negativeInteractionChanceFactor = NegativeInteractionUtility.NegativeInteractionChanceFactor(initiator, recipient);
			float deepTalkFactor = DeepTalkCompatibilityFactorCurve.Evaluate(initiator.relations.CompatibilityWith(recipient));

			weight *= weight_Chitchat
				+ deepTalkFactor * baseWeight_DeepTalk
				+ negativeInteractionChanceFactor * (baseWeight_Slight + baseWeight_Insult);

			//Log.Message("Returning weight: " + weight);
			return weight;
		}
	}

}
