using System.Collections.Generic;
using System.Linq;
using Core.Client.Helpers;
using dream_lib.src.extensions;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Features.MiningPoints.LandCleanup.Harvesters
{
	public class AiNavigationModule
	{

		private readonly NavMeshPath _navMeshPath;

		private Vector3[] _corners = new Vector3[0] ;
		private List<Vector3> _directions = new();
		private List<float> _distances = new();

		private float _currentLength;
		public Vector3 TargetPosition { get; private set; }

		public float Length { get; private set; }
		public float CompletionPercent => _currentLength / Length;
		public bool HasPath => _corners.Length > 0;

		public AiNavigationModule()
		{
			_navMeshPath = new NavMeshPath();
		}

		public void BuildPath(Vector3 charPos, Vector3 targetPosition)
		{
			var startPos = charPos;
			if(NavMesh.SamplePosition(startPos, out var hit, 10f, NavMesh.AllAreas))
			{
				startPos = hit.position;
			}
			if(NavMesh.SamplePosition(targetPosition, out hit, 10f, NavMesh.AllAreas))
			{
				targetPosition = hit.position;
			}
			if(!NavMesh.CalculatePath(startPos, targetPosition, NavMesh.AllAreas, _navMeshPath))
			{
				Debug.LogError("path calculation failed");
			}
			SetPath(_navMeshPath.corners);
		}

		public void SetPath(ICollection<Vector3> path)
		{
			_corners = path.ToArray();

			_currentLength = 0f;
			Length = 0f;
			_directions.Clear();
			_distances.Clear();

			for(var i = 0; i < _corners.Length; i++)
			{
				if(i < _corners.Length - 1)
				{
					_directions.Add(_corners[i + 1] - _corners[i]);
					_distances.Add(_directions[^1].magnitude);
					Length += _distances[^1];
				}
			}
			
			TargetPosition = _corners[^1];
		}

		public void DrawPath(Color color, float duration, float heightMarks = 0f)
		{
			for(var i = 1; i < _corners.Length; i++)
			{
				Debug.DrawLine(_corners[i - 1], _corners[i], color, duration, false);
				if(heightMarks > 0)
				{
					Debug.DrawLine(_corners[i], _corners[i] + Vector3.up * heightMarks, color, duration, true);
				}
			}
		}

		public Quaternion EvaluateRotation(float time, bool horizontal = true)
		{
			float precision = 0.1f / Length  ;
			var prevTime = Mathf.Max(0, time - precision);
			var nextTime = Mathf.Min(1, time + precision);
			var vector = EvaluatePos(nextTime) - EvaluatePos(prevTime);
			if(horizontal)
			{
				vector.y = 0;
			}
			
			return Quaternion.LookRotation(vector);
		}
		
		public Vector3 CalculateMoveDirection(Vector3 currentPosition, float precision = 0.1f, float offset = 0f)
		{
			currentPosition = currentPosition.SetY(0);
			if( _corners.Length <= 1)
			{
				return TargetPosition - currentPosition;
			}
			
			SampleByLength(_currentLength + offset, out var targetPos, out _);
			var dist = offset + precision;
			if((targetPos - currentPosition).SetY(0).sqrMagnitude < dist * dist)
			{
				_currentLength += precision * 1.1f;
				if(_currentLength > Length)
				{
					_currentLength = Length;
				}
				SampleByLength(_currentLength + offset, out targetPos, out _);
			}
			targetPos = targetPos.SetY(0);
			return (targetPos - currentPosition).normalized;
		}

		public Vector3 EvaluatePos(float time)
		{
			var iLength = 0f;
			var targetLength = Length * time;

			for(var i = 1; i < _corners.Length; i++)
			{
				var cornerLength = (_corners[i - 1] - _corners[i]).magnitude;
				if(iLength + cornerLength >= targetLength)
				{
					var cornerPercent = Mathf.Clamp01(1 - (iLength + cornerLength - targetLength) / cornerLength);
					var result = Vector3.Lerp(_corners[i - 1], _corners[i], cornerPercent);
					return result;
				}

				iLength += cornerLength;
			}

			return _corners[^1];
		}


		private void SampleByLength(float length, out Vector3 position, out Vector3 direction)
		{
			var iLength = 0f;
			for(var i = 0; i < _corners.Length - 1; i++)
			{
				var cornerDistance = _distances[i];
				if(iLength + cornerDistance >= length)
				{
					position = Vector3.Lerp(_corners[i], _corners[i + 1], (length - iLength) / cornerDistance);
					direction = _directions[i];

					return;
				}

				iLength += _distances[i];
			}

			position = _corners[^1];
			direction = _directions[^1];
		}

		public bool CheckIsCloseToFinish(float distanceToFinish)
		{
			return Length - _currentLength <= distanceToFinish;
		}
	}
}
