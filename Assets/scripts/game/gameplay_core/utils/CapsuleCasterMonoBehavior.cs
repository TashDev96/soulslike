using dream_lib.src.utils.data_types;
using dream_lib.src.utils.drawers;
using UnityEngine;

namespace game.gameplay_core.utils
{
	public class CapsuleCasterMonoBehavior : MonoBehaviour
	{
		[SerializeField]
		protected bool _drawDebug;

		[SerializeField]
		protected bool _drawSelected = true;

		[SerializeField]
		protected int _movementDirectionResolution = 5;

		[SerializeField]
		protected CapsuleCaster _capsuleCaster;

		private readonly RaycastHit[] _triggersCastResults = new RaycastHit[20];

		private Transform _transform;
		private int _triggersLayerMask;
		private Vector3[] _previousPositions;
		private Vector3[] _currentPositions;

		public TransformCache PreviousTransform { get; } = new();
		public TransformCache NextTransform { get; } = new();
		public float Radius => _capsuleCaster.Radius;

		private void Awake()
		{
			_transform = transform;
			_capsuleCaster.Transform = _transform;
			_triggersLayerMask = LayerMask.GetMask("Triggers");
			if(_previousPositions == null || _previousPositions.Length != _movementDirectionResolution)
			{
				_previousPositions = new Vector3[_movementDirectionResolution];
				_currentPositions = new Vector3[_movementDirectionResolution];
			}
		}

		public void GetCapsulePoints(out Vector3 p1, out Vector3 p2)
		{
			_capsuleCaster.GetCapsulePoints(transform.position, out p1, out p2);
		}

		public void GetCapsulePoints(Vector3 position, out Vector3 p1, out Vector3 p2)
		{
			_capsuleCaster.GetCapsulePoints(position, out p1, out p2);
		}

		public void DrawCollider(float duration, Color color)
		{
			_capsuleCaster.GetCapsulePoints(transform.position, out var p1, out var p2);
			DebugDrawUtils.DrawWireCapsulePersistent(p1, p2, _capsuleCaster.GetScaledRadius(), color, duration);
		}

		public void UpdateMovementDirectionCache(bool drawDebug = false, int stepCount = 0)
		{
			for(var i = 0; i < _movementDirectionResolution; i++)
			{
				_previousPositions[i] = _currentPositions[i];
			}

			_capsuleCaster.GetCapsulePoints(transform.position, out var p1, out var p2);
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
			PreviousTransform.Set(_transform);
		}

		private void OnDrawGizmosSelected()
		{
			if(!_drawSelected)
			{
				return;
			}
			if(!enabled)
			{
				return;
			}
			if(!_transform)
			{
				_transform = transform;
				_capsuleCaster.Transform = _transform;
			}
			_capsuleCaster.GetCapsulePoints(transform.position, out var p1, out var p2);
			DebugDrawUtils.DrawWireCapsule(p1, p2, _capsuleCaster.GetScaledRadius(), Color.white);
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
				_capsuleCaster.Transform = _transform;
			}

			OnDrawGizmosSelected();
		}
	}
}
