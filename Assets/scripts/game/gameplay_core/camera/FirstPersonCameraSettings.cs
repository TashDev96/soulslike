using UnityEngine;

namespace game.gameplay_core.camera
{
	public class FirstPersonCameraSettings : CameraSettings
	{
		[field: SerializeField]
		public Vector3 Offset { get; set; }
		[field: SerializeField]
		public float RotationSpeed { get; set; }
		[field: SerializeField]
		public float MinPitch { get; set; } = -80;
		[field: SerializeField]
		public float MaxPitch { get; set; } = 80;
		[field: SerializeField]
		public float FOV { get; set; }
	}
}
