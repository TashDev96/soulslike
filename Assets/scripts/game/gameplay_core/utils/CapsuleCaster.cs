using dream_lib.src.utils.drawers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace game.gameplay_core.utils
{
	public class CapsuleCaster : MonoBehaviour
	{
		public enum Direction
		{
			XAxis = 0,
			YAxis = 1,
			ZAxis = 2
		}

		[SerializeField]
		protected bool _drawDebug;

		[SerializeField]
		protected bool _useRotationConstraints;

		[SerializeField]
		[ShowIf(nameof(_useRotationConstraints))]
		protected bool _fixXRotation;

		[SerializeField]
		[ShowIf(nameof(_useRotationConstraints))]
		protected bool _fixYRotation;

		[SerializeField]
		[ShowIf(nameof(_useRotationConstraints))]
		protected bool _fixZRotation;

		[SerializeField]
		[ShowIf(nameof(_useRotationConstraints))]
		protected Vector3 _fixedRotation = Vector3.zero;
		public float Radius = 0.2f;
		public float Height = 2f;
		public Vector3 Center = Vector3.zero;
		public Direction CapsuleDirection = Direction.YAxis;

		private Transform _transform;

		private void Awake()
		{
			_transform = transform;
		}

		public void DrawCollider(float duration, Color color)
		{
			GetCapsulePoints(out var p1, out var p2);
			DebugDrawUtils.DrawWireCapsulePersistent(p1, p2, GetScaledRadius(), color, duration);
		}

		public void GetCapsulePoints(out Vector3 p1, out Vector3 p2)
		{
			GetCapsulePoints(_transform.position, out p1, out p2);
		}

		public void GetCapsulePoints(Vector3 position, out Vector3 p1, out Vector3 p2)
		{
			var scaledRadius = GetScaledRadius();
			var scaledHeight = GetScaledHeight();

			var localHeight = Mathf.Max(scaledHeight, scaledRadius * 2f);
			var half = localHeight * 0.5f - scaledRadius;

			var directionVector = GetDirectionVector();
			Quaternion rotation;

			if(_useRotationConstraints)
			{
				rotation = GetConstrainedRotation();
			}
			else
			{
				rotation = _transform.rotation;
			}

			var direction = rotation * directionVector;
			var scaledCenter = GetScaledCenter();

			p1 = position + direction * half + rotation * scaledCenter;
			p2 = position - direction * half + rotation * scaledCenter;
		}

		protected Vector3 GetDirectionVector()
		{
			switch(CapsuleDirection)
			{
				case Direction.XAxis:
					return Vector3.right;
				case Direction.YAxis:
					return Vector3.up;
				case Direction.ZAxis:
					return Vector3.forward;
				default:
					return Vector3.up;
			}
		}

		private float GetScaledRadius()
		{
			var scale = _transform.lossyScale;

			switch(CapsuleDirection)
			{
				case Direction.XAxis:
					return Radius * Mathf.Max(scale.y, scale.z);
				case Direction.YAxis:
					return Radius * Mathf.Max(scale.x, scale.z);
				case Direction.ZAxis:
					return Radius * Mathf.Max(scale.x, scale.y);
				default:
					return Radius * Mathf.Max(scale.x, scale.z);
			}
		}

		private float GetScaledHeight()
		{
			var scale = _transform.lossyScale;

			switch(CapsuleDirection)
			{
				case Direction.XAxis:
					return Height * scale.x;
				case Direction.YAxis:
					return Height * scale.y;
				case Direction.ZAxis:
					return Height * scale.z;
				default:
					return Height * scale.y;
			}
		}

		private Vector3 GetScaledCenter()
		{
			return Vector3.Scale(Center, _transform.lossyScale);
		}

		private Quaternion GetConstrainedRotation()
		{
			var eulerAngles = _transform.rotation.eulerAngles;

			if(_fixXRotation)
			{
				eulerAngles.x = _fixedRotation.x;
			}

			if(_fixYRotation)
			{
				eulerAngles.y = _fixedRotation.y;
			}

			if(_fixZRotation)
			{
				eulerAngles.z = _fixedRotation.z;
			}

			return Quaternion.Euler(eulerAngles);
		}

		private void OnDrawGizmosSelected()
		{
			if(!_transform)
			{
				_transform = transform;
			}
			GetCapsulePoints(out var p1, out var p2);
			DebugDrawUtils.DrawWireCapsule(p1, p2, GetScaledRadius(), Color.white);
		}

		private void OnDrawGizmos()
		{
			if(!_drawDebug)
			{
				return;
			}

			if(!_transform)
			{
				_transform = transform;
			}

			OnDrawGizmosSelected();
		}
	}
}
