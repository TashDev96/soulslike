using System;
using UnityEngine;

namespace game.gameplay_core.characters
{
	[Serializable]
	public class RotationSpeedData
	{
		[SerializeField]
		private float _halfTurnDurationSeconds;

		public float HalfTurnDurationSeconds => _halfTurnDurationSeconds;
		public float DegreesPerSecond => 180f / _halfTurnDurationSeconds;
	}
}
