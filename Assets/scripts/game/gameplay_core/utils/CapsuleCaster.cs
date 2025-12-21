using dream_lib.src.utils.data_types;
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

		[SerializeField]
		protected int _movementDirectionResolution = 5;
		public float Radius = 0.2f;
		public float Height = 2f;
		public Vector3 Center = Vector3.zero;
		public Direction CapsuleDirection = Direction.YAxis;

		private readonly RaycastHit[] _triggersCastResults = new RaycastHit[20];

		private Transform _transform;
		private int _triggersLayerMask;
		private Vector3[] _previousPositions;
		private Vector3[] _currentPositions;

		public TransformCache PreviousTransform { get; } = new();
		public TransformCache NextTransform { get; } = new();

		private void Awake()
		{
			_transform = transform;
			_triggersLayerMask = LayerMask.GetMask("Triggers");
			if(_previousPositions == null || _previousPositions.Length != _movementDirectionResolution)
			{
				_previousPositions = new Vector3[_movementDirectionResolution];
				_currentPositions = new Vector3[_movementDirectionResolution];
			}
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

		public void UpdateMovementDirectionCache(bool drawDebug = false, int stepCount = 0)
		{
			for(var i = 0; i < _movementDirectionResolution; i++)
			{
				_previousPositions[i] = _currentPositions[i];
			}

			GetCapsulePoints(out var p1, out var p2);
			var axisDirection = (p2 - p1).normalized;
			var axisLength = (p2 - p1).magnitude;

			for(var i = 0; i < _movementDirectionResolution; i++)
			{
				var t = _movementDirectionResolution > 1 ? (float)i / (_movementDirectionResolution - 1) : 0f;
				_currentPositions[i] = p1 + axisDirection * (axisLength * t);

				if(drawDebug)
				{
					DebugDrawUtils.DrawArrow(_previousPositions[i], _currentPositions[i], Color.green, 0.05f, 20f, 0f, false);
				}
			}
		}

		public Vector3 SampleMoveDirection(Vector3 worldPosition, bool normalized = true)
		{
			if(_currentPositions == null || _currentPositions.Length == 0)
			{
				return Vector3.zero;
			}

			var closestIndex1 = 0;
			var closestIndex2 = 0;
			var minDistSq1 = float.MaxValue;
			var minDistSq2 = float.MaxValue;

			for(var i = 0; i < _currentPositions.Length; i++)
			{
				var distSq = (worldPosition - _currentPositions[i]).sqrMagnitude;
				if(distSq < minDistSq1)
				{
					minDistSq2 = minDistSq1;
					closestIndex2 = closestIndex1;
					minDistSq1 = distSq;
					closestIndex1 = i;
				}
				else if(distSq < minDistSq2)
				{
					minDistSq2 = distSq;
					closestIndex2 = i;
				}
			}

			var totalDistSq = minDistSq1 + minDistSq2;
			if(totalDistSq < 0.0001f)
			{
				return Vector3.zero;
			}

			var weight1 = minDistSq2 / totalDistSq;
			var weight2 = minDistSq1 / totalDistSq;

			var direction1 = _currentPositions[closestIndex1] - _previousPositions[closestIndex1];
			var direction2 = _currentPositions[closestIndex2] - _previousPositions[closestIndex2];
			var resultVector = direction1 * weight1 + direction2 * weight2;
			return normalized ? resultVector.normalized : resultVector;
		}

		public void StorePreviousPosition(bool drawDebug = false)
		{
			if(drawDebug)
			{
				var prevPos = PreviousTransform.Position;
				var nextPos = _transform.position;
			}
			PreviousTransform.Set(_transform);
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
			if(!enabled)
			{
				return;
			}
			if(!_transform)
			{
				_transform = transform;
			}
			GetCapsulePoints(out var p1, out var p2);
			DebugDrawUtils.DrawWireCapsule(p1, p2, GetScaledRadius(), Color.white);
		}

		private void OnDrawGizmos()
		{
			if(!_drawDebug || !enabled)
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
