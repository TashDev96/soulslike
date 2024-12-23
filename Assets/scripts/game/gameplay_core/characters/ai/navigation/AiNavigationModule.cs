using dream_lib.src.utils.data_types;
using UnityEngine;
using UnityEngine.AI;

namespace game.gameplay_core.characters.ai.navigation
{
	public class AiNavigationModule
	{
		private readonly ReadOnlyTransform _characterTransform;

		private readonly NavMeshPath _navMeshPath;

		private int _currentIndex;

		public AiNavigationModule(ReadOnlyTransform characterTransform)
		{
			Path = new PathWrapper();
			_navMeshPath = new NavMeshPath();
			_characterTransform = characterTransform;
		}

		public PathWrapper Path { get; }

		public void BuildPath(Vector3 targetPosition)
		{
			NavMesh.CalculatePath(_characterTransform.Position, targetPosition, NavMesh.AllAreas, _navMeshPath);
			if(_navMeshPath.status != NavMeshPathStatus.PathInvalid)
			{
				Path.SetPath(_navMeshPath);
			}

			_currentIndex = 0;
		}

		public Vector3 CalculateMoveDirection(Vector3 currentPosition)
		{
			if(_currentIndex >= Path.Positions.Count - 1)
			{
				return (Path.Positions[^1] - currentPosition).normalized;
			}

			var currentPoint = Path.Positions[_currentIndex];
			var nextPoint = Path.Positions[_currentIndex + 1];

			var vectorBetweenPoints = nextPoint - currentPoint;
			var vectorOfMotion = currentPosition - currentPoint;


			if(Vector3.Project(vectorOfMotion, vectorBetweenPoints).magnitude > vectorBetweenPoints.magnitude)
			{
				_currentIndex++;
				nextPoint = Path.Positions[Mathf.Min(Path.Positions.Count - 1, _currentIndex + 1)];
			}

			
			//Debug.DrawLine(currentPosition, currentPosition + (nextPoint - currentPosition).normalized * 2f, Color.green);


			return (nextPoint - currentPosition).normalized;
		}
	}
}