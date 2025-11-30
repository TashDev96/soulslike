using UnityEngine;

namespace game.gameplay_core.camera
{
	public class IsometricCameraSettings : CameraSettings
	{
		[SerializeField]
		private float _cameraAltitude;
		[SerializeField]
		private float _occlusionCheckRadius = 0.1f;
		[SerializeField]
		private LayerMask _occlusionCheckLayerMask;
		[SerializeField]
		private float _occlusionFadeSpeed = 5f;

		public float CameraAltitude => _cameraAltitude;
		public float OcclusionCheckRadius => _occlusionCheckRadius;
		public LayerMask OcclusionCheckLayerMask => _occlusionCheckLayerMask;
		public float OcclusionFadeSpeed => _occlusionFadeSpeed;
	}
}
