using UnityEngine;

namespace game.gameplay_core.camera
{
	public class ThirdPersonCameraSettings : CameraSettings
	{
		[SerializeField]
		private float _distance = 5f;

		[SerializeField]
		private float _heightOffset = 1.5f;

		[SerializeField]
		private float _followSpeed = 10f;

		[SerializeField]
		private float _rotationSpeed = 180f;

		[SerializeField]
		private float _autoRotateSpeed = 2f;

		[SerializeField]
		private float _minPitch = -80f;

		[SerializeField]
		private float _maxPitch = 80f;

		[SerializeField]
		private LayerMask _obstacleLayerMask;

		[SerializeField]
		private float _obstacleCheckRadius = 0.2f;

		[SerializeField]
		private Vector3 _pivotOffset = Vector3.zero;

		public float Distance => _distance;
		public float HeightOffset => _heightOffset;
		public float FollowSpeed => _followSpeed;
		public float RotationSpeed => _rotationSpeed;
		public float AutoRotateSpeed => _autoRotateSpeed;
		public float MinPitch => _minPitch;
		public float MaxPitch => _maxPitch;
		public LayerMask ObstacleLayerMask => _obstacleLayerMask;
		public float ObstacleCheckRadius => _obstacleCheckRadius;
		public Vector3 PivotOffset => _pivotOffset;
	}
}
