using System;
using UnityEngine;

namespace game.gameplay_core.characters.config.animation
{
	[Serializable]
	public class AnimEventFlapWings : AnimationEventBase
	{
		[field: SerializeField]
		public AnimationCurve LiftForce { get; private set; }
		
		[field:SerializeField]
		public AnimationCurve PitchCurve { get; private set; } = new (new Keyframe(-1f, 0f), new Keyframe(1f, 80f));


		
		[field:SerializeField]
		public Vector3 LiftForceAxesMultipliers { get; private set; } = new (1, 1, 1);

		public float GetLiftForce(float normalizedAnimationTime)
		{
			if(normalizedAnimationTime < StartTimeNormalized || normalizedAnimationTime > EndTimeNormalized)
			{
				return 0;
			}
			
			var mappedTime = (normalizedAnimationTime - StartTimeNormalized) / (EndTimeNormalized - StartTimeNormalized);
			return LiftForce.Evaluate(mappedTime);
		}
	}
}
