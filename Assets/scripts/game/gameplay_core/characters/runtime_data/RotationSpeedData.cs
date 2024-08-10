using System;
using UnityEngine;

namespace game.gameplay_core.characters.runtime_data
{
	[Serializable]
	public struct RotationSpeedData
	{
		[SerializeField]
		private float _halfTurnDurationSeconds;

		public float HalfTurnDurationSeconds => _halfTurnDurationSeconds;
		public float DegreesPerSecond => 180f / _halfTurnDurationSeconds;
	}
}
