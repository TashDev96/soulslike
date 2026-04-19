using System;
using UnityEngine;

namespace game.gameplay_core.utils
{
	[Serializable]
	public class CapsuleCaster
	{
		public enum Direction
		{
			XAxis = 0,
			YAxis = 1,
			ZAxis = 2
		}
		
		public Transform Transform;
		public float Radius = 0.2f;
		public float Height = 2f;
		public Vector3 Center = Vector3.zero;
		public Direction CapsuleDirection = Direction.YAxis;

		 

		public void GetCapsulePoints(Vector3 position, out Vector3 p1, out Vector3 p2)
		{
			var scaledRadius = GetScaledRadius();
			var scaledHeight = GetScaledHeight();

			var localHeight = Mathf.Max(scaledHeight, scaledRadius * 2f);
			var half = localHeight * 0.5f - scaledRadius;

			var directionVector = GetDirectionVector();
			Quaternion rotation;

			rotation = Transform.rotation;

			var direction = rotation * directionVector;
			var scaledCenter = GetScaledCenter();

			p1 = position + direction * half + rotation * scaledCenter;
			p2 = position - direction * half + rotation * scaledCenter;
		}

		public float GetScaledRadius()
		{
			var scale = Transform.lossyScale;

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

		private float GetScaledHeight()
		{
			var scale = Transform.lossyScale;

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
			return Vector3.Scale(Center, Transform.lossyScale);
		}
	}
}
