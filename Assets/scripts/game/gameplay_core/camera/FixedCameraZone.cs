using System;
using UnityEngine;

namespace game.gameplay_core.camera
{
	public enum CameraZoneBehavior
	{
		Fixed,
		LookAtPlayer,
		FollowPlayer
	}

	[Serializable]
	public class FixedCameraZone
	{
		[SerializeField]
		private BoxCollider _trigger;

		[SerializeField]
		private Transform _cameraTransform;

		[SerializeField]
		private CameraZoneBehavior _behavior;

		[SerializeField]
		private float _followSmoothness = 5f;

		[SerializeField]
		private float _rotationSmoothness = 5f;

		[SerializeField]
		private Vector3 _lookAtOffset = new(0, 1.5f, 0);

		public BoxCollider Trigger => _trigger;
		public Transform CameraTransform => _cameraTransform;
		public CameraZoneBehavior Behavior => _behavior;
		public float FollowSmoothness => _followSmoothness;
		public float RotationSmoothness => _rotationSmoothness;
		public Vector3 LookAtOffset => _lookAtOffset;
	}
}
