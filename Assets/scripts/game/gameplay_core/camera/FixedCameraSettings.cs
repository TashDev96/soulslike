using System.Collections.Generic;
using UnityEngine;

namespace game.gameplay_core.camera
{
	public class FixedCameraSettings : CameraSettings
	{
		[SerializeField]
		private List<FixedCameraZone> _zones = new();

		[SerializeField]
		private float _transitionDuration = 1f;

		public IReadOnlyList<FixedCameraZone> Zones => _zones;
		public float TransitionDuration => _transitionDuration;
	}
}
