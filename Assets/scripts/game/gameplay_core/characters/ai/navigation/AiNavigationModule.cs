using dream_lib.src.utils.data_types;
using UnityEngine;
using UnityEngine.AI;

namespace game.gameplay_core.characters.ai.navigation
{
	public class AiNavigationModule
	{
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
			NavMesh.SamplePosition(targetPosition, out var hit, 3f, NavMesh.AllAreas);
			TargetPosition = hit.position;
			NavMesh.CalculatePath(_characterTransform.Position, targetPosition, NavMesh.AllAreas, _navMeshPath);
			if(_navMeshPath.status != NavMeshPathStatus.PathInvalid)
			{
				Path.SetPath(_navMeshPath);
			}

			_currentLength = 0;
		}

		public bool CheckTargetPositionChangedSignificantly(Vector3 newTargetPosition, float mean = 0.1f)
		{
			return (TargetPosition - newTargetPosition).sqrMagnitude > mean * mean;
		}

		public Vector3 CalculateMoveDirection(Vector3 currentPosition)
		{
			SampleByLength(_currentLength, out var targetPos, out _);
			if((targetPos - currentPosition).sqrMagnitude < 0.1f * 0.1f)
			{
				_currentLength += 0.2f;
				SampleByLength(_currentLength, out targetPos, out _);
			}

			return (targetPos - currentPosition).normalized;
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
