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
	public class FixedCameraZone : MonoBehaviour
	{
		[field: SerializeField]
		public BoxCollider Trigger { get; private set; }
		[field: SerializeField]
		public Transform CameraTransform { get; private set; }
		[field: SerializeField]
		public CameraZoneBehavior Behavior { get; private set; }
		[field: SerializeField]
		public float FollowSmoothness { get; private set; }
		[field: SerializeField]
		public float RotationSmoothness { get; private set; }
		[field: SerializeField]
		public Vector3 LookAtOffset { get; private set; }
		[field: SerializeField]
		public int Priority { get; private set; } = 1;

		private void OnValidate()
		{
			if(Trigger == null)
			{
				Trigger = GetComponentInChildren<BoxCollider>();
			}
		}
	}
}
