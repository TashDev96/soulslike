using Sirenix.OdinInspector;
using UnityEngine;

namespace game.gameplay_core.camera
{
	public class IsometricCameraSettings : CameraSettings
	{
		[field:SerializeField]
		public PerspectiveMode Mode { get; private set; } = PerspectiveMode.Orthographic;
		[field:SerializeField]
		[field:ShowIf("@Mode == PerspectiveMode.Perspective")]
		public float FOV { get; private set; } = 15f;
		[field:SerializeField]
		[field:ShowIf("@Mode == PerspectiveMode.Orthographic")]
		public float OrthoSize { get; private set; } = 15f;
		
		[field: SerializeField]
		public float CameraAltitude { get; private set; } = 80;
		[field: SerializeField]
		public float OcclusionCheckRadius { get; private set; } = 0.1f;
		[field: SerializeField]
		public LayerMask OcclusionCheckLayerMask { get; private set; }
		[field: SerializeField]
		public float OcclusionFadeSpeed { get; private set; } = 5f;
		[field: SerializeField]
		public AnimationCurve CriticalAttackZoomMultiplier { get; private set; }


		public enum PerspectiveMode
		{
			Orthographic,
			Perspective
		}
	}
}
