using UnityEngine;

namespace game.gameplay_core.camera
{
	public abstract class CameraSettings : MonoBehaviour
	{
		[SerializeField]
		private float _occlusionSphereRadius = 1.5f;
		[SerializeField]
		private float _occlusionSphereHeightOffset = 1.0f;
		[SerializeField]
		private float _occlusionCircleOffset = 0.5f;

		public static Vector3 GizmoCircleCenter;
		public static Vector3 GizmoCircleNormal;
		public static float GizmoCircleRadius;

		public float OcclusionSphereRadius => _occlusionSphereRadius;
		public float OcclusionSphereHeightOffset => _occlusionSphereHeightOffset;
		public float OcclusionCircleOffset => _occlusionCircleOffset;

		private void OnDrawGizmos()
		{
			if(GizmoCircleNormal.sqrMagnitude < 0.001f)
			{
				return;
			}

			Gizmos.color = Color.cyan;
			DrawWireCircle(GizmoCircleCenter, GizmoCircleNormal, GizmoCircleRadius, 32);
		}

		private void DrawWireCircle(Vector3 center, Vector3 normal, float radius, int segments)
		{
			var rotation = Quaternion.LookRotation(normal);
			var right = rotation * Vector3.right;
			var up = rotation * Vector3.up;

			var prevPoint = center + right * radius;
			for(var i = 1; i <= segments; i++)
			{
				var angle = i / (float)segments * Mathf.PI * 2;
				var nextPoint = center + (right * Mathf.Cos(angle) + up * Mathf.Sin(angle)) * radius;
				Gizmos.DrawLine(prevPoint, nextPoint);
				prevPoint = nextPoint;
			}
		}
	}
}
