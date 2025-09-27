using System;
using FlowField;
using Sirenix.OdinInspector;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace experiments
{
	public class PathFindManager : MonoBehaviour
	{
		[SerializeField]
		private Bounds _worldBounds;
		[SerializeField]
		private int _gridResolution = 50;

		[SerializeField]
		private bool _showDebugVisualization = true;
		[SerializeField]
		private float _debugArrowLength = 1f;
		[SerializeField]
		private float _debugHeight = 0.1f;
		[SerializeField]
		private float _raycastHeight = 10f;
		[SerializeField]
		private LayerMask _groundLayerMask = -1;
		private Vector2Int targetPos;
		private NativeArray<double2> _direction;
		private NativeArray<double> _gradient;
		private Vector2 _gridCellSize;
		private Vector2 _gridOrigin;
		public static PathFindManager Instance { get; private set; }

		private void Awake()
		{
			if(Instance == null)
			{
				Instance = this;
				DontDestroyOnLoad(gameObject);
			}
			else
			{
				Destroy(gameObject);
			}
		}

		public void SetTargetPosition(Vector3 worldPosition)
		{
			targetPos = WorldToGridPosition(worldPosition);
		}

		[Button]
		public void SetupGrid(Bounds worldBounds, int gridResolution)
		{
			_worldBounds = worldBounds;
			_gridResolution = gridResolution;
		}

		[Button]
		public void CalculateFlow()
		{
			var size = _gridResolution;
			_gridCellSize = new Vector2(
				_worldBounds.size.x / _gridResolution,
				_worldBounds.size.z / _gridResolution
			);

			_gridOrigin = new Vector2(
				_worldBounds.min.x,
				_worldBounds.min.z
			);

			var speeds = new float[size + 2, size + 2];

			for(var x = 1; x <= size; x++)
			{
				for(var y = 1; y <= size; y++)
				{
					var worldPos = GridToWorldPosition(x - 1, y - 1);
					var hasNavMesh = SampleNavMeshAtPosition(worldPos);

					speeds[x, y] = hasNavMesh ? 1 : -1f;
				}
			}

			for(var i = 0; i <= size; i++)
			{
				speeds[0, i] = speeds[size + 1, i] = speeds[i, 0] = speeds[i, size + 1] = 0;
			}

			TimeSpan timeSpan;
			(_direction, _gradient, timeSpan) = FlowCalculationController.RequestCalculation(speeds, targetPos, _gridResolution, _gridResolution);
		}

		public Vector2Int WorldToGridPosition(Vector3 worldPosition)
		{
			var localPos = new Vector2(
				worldPosition.x - _gridOrigin.x,
				worldPosition.z - _gridOrigin.y
			);

			var gridX = Mathf.FloorToInt(localPos.x / _gridCellSize.x);
			var gridY = Mathf.FloorToInt(localPos.y / _gridCellSize.y);

			gridX = Mathf.Clamp(gridX, 0, _gridResolution - 1);
			gridY = Mathf.Clamp(gridY, 0, _gridResolution - 1);

			return new Vector2Int(gridX, gridY);
		}

		public Vector3 GridToWorldPosition(int gridX, int gridY)
		{
			var worldX = _gridOrigin.x + (gridX + 0.5f) * _gridCellSize.x;
			var worldZ = _gridOrigin.y + (gridY + 0.5f) * _gridCellSize.y;
			return new Vector3(worldX, 0, worldZ);
		}

		public Vector2 SampleFlowDirection(Vector3 worldPosition)
		{
			if(!_direction.IsCreated)
			{
				return Vector2.zero;
			}

			var gridPos = WorldToGridPosition(worldPosition);
			var index = (gridPos.y + 1) * (_gridResolution + 2) + gridPos.x + 1;

			if(index >= 0 && index < _direction.Length)
			{
				var direction = _direction[index];
				return new Vector2((float)direction.x, (float)direction.y);
			}

			var targetWorldPos = GridToWorldPosition(targetPos.x, targetPos.y);
			return targetWorldPos - worldPosition;
		}

		public float SampleFlowGradient(Vector3 worldPosition)
		{
			if(!_gradient.IsCreated)
			{
				return 0f;
			}

			var gridPos = WorldToGridPosition(worldPosition);
			var index = (gridPos.y + 1) * (_gridResolution + 2) + gridPos.x + 1;

			if(index >= 0 && index < _gradient.Length)
			{
				return (float)_gradient[index];
			}

			return 0f;
		}

		public void DrawDebugVisualization()
		{
			if(!_showDebugVisualization || !_direction.IsCreated)
			{
				return;
			}

			for(var x = 0; x < _gridResolution; x++)
			{
				for(var y = 0; y < _gridResolution; y++)
				{
					var worldPos = GridToWorldPosition(x, y);
					var index = (y + 1) * (_gridResolution + 2) + x + 1;

					if(index >= 0 && index < _direction.Length)
					{
						var direction = _direction[index];
						var dir2D = new Vector2((float)direction.x, (float)direction.y);

						if(dir2D.magnitude > 0.01f)
						{
							var startPos = new Vector3(worldPos.x, _debugHeight, worldPos.z);
							var endPos = startPos + new Vector3(dir2D.x, 0, dir2D.y) * _debugArrowLength;

							var color = _gradient[index] < 0 ? Color.red : Color.green;
							Debug.DrawLine(startPos, endPos, color);

							var arrowHeadSize = _debugArrowLength * 0.2f;
							var arrowRight = Vector3.Cross(Vector3.up, endPos - startPos).normalized * arrowHeadSize;
							var arrowBack = (startPos - endPos).normalized * arrowHeadSize;

							Debug.DrawLine(endPos, endPos + arrowRight + arrowBack, color);
							Debug.DrawLine(endPos, endPos - arrowRight + arrowBack, color);
						}
					}
				}
			}
		}

		private bool SampleNavMeshAtPosition(Vector3 worldPosition)
		{
			var raycastStart = worldPosition + Vector3.up * _raycastHeight;

			if(Physics.Raycast(raycastStart, Vector3.down, out var groundHit, _raycastHeight * 2f, _groundLayerMask))
			{
				var result = NavMesh.SamplePosition(groundHit.point, out var navHit, 0.2f, NavMesh.AllAreas);
				Debug.DrawLine(groundHit.point, groundHit.point + Vector3.up, result ? Color.green : Color.red, 5);

				return result;
			}

			return false;
		}

		private void Update()
		{
			DrawDebugVisualization();
		}

		private void OnDestroy()
		{
			if(_direction.IsCreated)
			{
				_direction.Dispose();
			}
			if(_gradient.IsCreated)
			{
				_gradient.Dispose();
			}
		}
	}
}
