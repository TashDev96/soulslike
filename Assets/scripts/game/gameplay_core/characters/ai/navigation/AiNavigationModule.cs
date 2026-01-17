using dream_lib.src.utils.data_types;
using UnityEngine;
using UnityEngine.AI;

namespace game.gameplay_core.characters.ai.navigation
{
	public class AiNavigationModule
	{
		private const float NavMeshSampleRadius = 2f;
		private const float ArrivalThreshold = 0.2f;

		public Vector3 TargetPosition;
		private readonly ReadOnlyTransform _characterTransform;

		private readonly NavMeshPath _navMeshPath;

		private float _currentLength;

		public PathWrapper Path { get; }

		public AiNavigationModule(ReadOnlyTransform characterTransform)
		{
			Path = new PathWrapper();
			_navMeshPath = new NavMeshPath();
			_characterTransform = characterTransform;
		}

		public void BuildPath(Vector3 targetPosition)
		{
			TargetPosition = targetPosition;

			var startPos = _characterTransform.Position;
			if(NavMesh.SamplePosition(startPos, out var startHit, NavMeshSampleRadius, NavMesh.AllAreas))
			{
				startPos = startHit.position;
			}

			var endPos = targetPosition;
			if(NavMesh.SamplePosition(endPos, out var endHit, NavMeshSampleRadius, NavMesh.AllAreas))
			{
				endPos = endHit.position;
			}

			NavMesh.CalculatePath(startPos, endPos, NavMesh.AllAreas, _navMeshPath);
			if(_navMeshPath.status != NavMeshPathStatus.PathInvalid)
			{
				Path.SetPath(_navMeshPath);
			}

			_currentLength = 0;
		}

		public bool CheckTargetPositionChangedSignificantly(Vector3 newTargetPosition, float threshold = 0.1f)
		{
			return (TargetPosition - newTargetPosition).sqrMagnitude > threshold * threshold;
		}

		public Vector3 CalculateMoveDirection(Vector3 currentPosition)
		{
			SampleByLength(_currentLength, out var targetPos, out _);

			var diff = targetPos - currentPosition;
			var diff2D = new Vector2(diff.x, diff.z);

			if(diff2D.sqrMagnitude < ArrivalThreshold * ArrivalThreshold)
			{
				_currentLength += ArrivalThreshold;
				SampleByLength(_currentLength, out targetPos, out _);
				diff = targetPos - currentPosition;
				diff2D = new Vector2(diff.x, diff.z);
			}

			if(diff2D.sqrMagnitude < Mathf.Epsilon)
			{
				return Vector3.zero;
			}

			return new Vector3(diff2D.x, 0, diff2D.y).normalized;
		}

		public void DrawDebug(Color color, float duration = 0)
		{
			for(var i = 1; i < Path.Positions.Count; i++)
			{
				var prevPos = Path.Positions[i - 1];
				var pos = Path.Positions[i];
				Debug.DrawLine(prevPos, pos, color, duration);
			}
		}

		private void SampleByLength(float length, out Vector3 position, out Vector3 direction)
		{
			var iLength = 0f;
			for(var i = 0; i < Path.Positions.Count - 1; i++)
			{
				var cornerDistance = Path.Distances[i];
				if(iLength + cornerDistance >= length)
				{
					position = Vector3.Lerp(Path.Positions[i], Path.Positions[i + 1], (length - iLength) / cornerDistance);
					direction = Path.Directions[i];
					return;
				}

				iLength += Path.Distances[i];
			}

			if(Path.Positions.Count == 0)
			{
				position = Vector3.zero;
				direction = Vector3.forward;
				return;
			}

			position = Path.Positions[^1];
			direction = Path.Directions[^1];
		}
	}
}
