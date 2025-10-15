using System;
using dream_lib.src.utils.drawers;
using FlowField;
using Sirenix.OdinInspector;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace experiments
{
	public class PathFindManager : MonoBehaviour
	{
		[SerializeField]
		private Bounds _worldBounds;
		[SerializeField]
		private int _gridResolution = 50;

		[Header("Walkability settings")]
		[SerializeField]
		private float _walkableAltitude;
		[SerializeField]
		private float _walkableAltitudeTolerance = 0.1f;
		[SerializeField]
		private float _sampleWallsSphereRadius = 3f;
		[SerializeField]
		private float _closeToWallsPenalty = 0.5f;

		[Header("Debug settings")]
		[SerializeField]
		private bool _showDebugVisualization = true;
		[SerializeField]
		private float _debugArrowLength = 1f;
		[SerializeField]
		private float _debugHeight = 0.1f;
		[SerializeField]
		private float _raycastHeight = 50f;
		[SerializeField]
		private LayerMask _groundLayerMask = -1;
		private Vector2Int targetPos;
		private NativeArray<double2> _direction;
		private NativeArray<double> _gradient;
		private NativeArray<double2> _secondLayerDirection;
		private Vector2 _gridCellSize;
		private Vector2 _gridOrigin;

		public static PathFindManager Instance { get; private set; }

		private void InitializeSecondLayer()
		{
			if(_secondLayerDirection.IsCreated)
			{
				_secondLayerDirection.Dispose();
			}

			var totalSize = (_gridResolution + 2) * (_gridResolution + 2);
			_secondLayerDirection = new NativeArray<double2>(totalSize, Allocator.Persistent);
		}

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
			SetupGrid(_worldBounds, _gridResolution);
			
		}

		private void Start()
		{
			CalculateFlow();
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
			_gridCellSize = new Vector2(
				_worldBounds.size.x / _gridResolution,
				_worldBounds.size.z / _gridResolution
			);

			_gridOrigin = new Vector2(
				_worldBounds.min.x,
				_worldBounds.min.z
			);
		}

		[Button]
		public void CalculateFlow()
		{
			var size = _gridResolution;
			SetupGrid(_worldBounds, _gridResolution);
			

			InitializeSecondLayer();

			var speeds = new float[size + 2, size + 2];

			for(var x = 1; x <= size; x++)
			{
				for(var y = 1; y <= size; y++)
				{
					var worldPos = GridToWorldPosition(x - 1, y - 1);

					speeds[x, y] = GetCellSpeed(worldPos);
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

		public Vector2 SampleFlowDirection(Vector3 worldPosition, bool includeSecondLayer = false)
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
				var result = new Vector2((float)direction.x, (float)direction.y);

				if(includeSecondLayer && _secondLayerDirection.IsCreated && index < _secondLayerDirection.Length)
				{
					var secondLayerDir = _secondLayerDirection[index];
					result += new Vector2((float)secondLayerDir.x, (float)secondLayerDir.y);
				}

				return result;
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
						var direction = math.normalize(_direction[index] + _secondLayerDirection[index]);
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

		public void SetRadialForce(Vector3 worldPos, float radius, float centerForce, float endForce)
		{
			if(!_secondLayerDirection.IsCreated)
			{
				return;
			}

			var centerGridPos = WorldToGridPosition(worldPos);
			var radiusInGridCells = Mathf.CeilToInt(radius / Mathf.Min(_gridCellSize.x, _gridCellSize.y));

			for(var x = Mathf.Max(0, centerGridPos.x - radiusInGridCells); x <= Mathf.Min(_gridResolution - 1, centerGridPos.x + radiusInGridCells); x++)
			{
				for(var y = Mathf.Max(0, centerGridPos.y - radiusInGridCells); y <= Mathf.Min(_gridResolution - 1, centerGridPos.y + radiusInGridCells); y++)
				{
					var cellWorldPos = GridToWorldPosition(x, y);
					var distance = Vector3.Distance(worldPos, cellWorldPos);

					if(distance <= radius)
					{
						var normalizedDistance = distance / radius;
						var force = Mathf.Lerp(centerForce, endForce, normalizedDistance);

						var direction = (cellWorldPos - worldPos).normalized;
						var forceVector = new Vector2(direction.x, direction.z) * force;

						var index = (y + 1) * (_gridResolution + 2) + x + 1;
						if(index >= 0 && index < _secondLayerDirection.Length)
						{
							_secondLayerDirection[index] = new double2(forceVector.x, forceVector.y);
						}
					}
				}
			}
		}

		public void SetDirectionalForce(Vector3 worldPos, Vector3 direction, float radius, float centerForce, float endForce, float frontAngle = 90f)
		{
			if(!_secondLayerDirection.IsCreated)
			{
				return;
			}

			var centerGridPos = WorldToGridPosition(worldPos);
			var radiusInGridCells = Mathf.CeilToInt(radius / Mathf.Min(_gridCellSize.x, _gridCellSize.y));
			var normalizedDirection = direction.normalized;
			var forwardDir2D = new Vector2(normalizedDirection.x, normalizedDirection.z);
			var rightDir2D = new Vector2(-normalizedDirection.z, normalizedDirection.x);

			for(var x = Mathf.Max(0, centerGridPos.x - radiusInGridCells); x <= Mathf.Min(_gridResolution - 1, centerGridPos.x + radiusInGridCells); x++)
			{
				for(var y = Mathf.Max(0, centerGridPos.y - radiusInGridCells); y <= Mathf.Min(_gridResolution - 1, centerGridPos.y + radiusInGridCells); y++)
				{
					var cellWorldPos = GridToWorldPosition(x, y);
					var distance = Vector3.Distance(worldPos, cellWorldPos);

					if(distance <= radius)
					{
						var normalizedDistance = distance / radius;
						var force = Mathf.Lerp(centerForce, endForce, normalizedDistance);

						var toCellDir = (cellWorldPos - worldPos).normalized;
						var toCellDir2D = new Vector2(toCellDir.x, toCellDir.z);

						var dotProduct = Vector2.Dot(forwardDir2D, toCellDir2D);
						var angle = Mathf.Acos(Mathf.Clamp(dotProduct, -1f, 1f)) * Mathf.Rad2Deg;

						Vector2 forceVector;
						if(angle <= frontAngle * 0.5f)
						{
							var sideDirection = Vector2.Dot(rightDir2D, toCellDir2D) > 0 ? rightDir2D : -rightDir2D;
							forceVector = sideDirection * force;
						}
						else
						{
							forceVector = toCellDir2D * force;
						}

						var index = (y + 1) * (_gridResolution + 2) + x + 1;
						if(index >= 0 && index < _secondLayerDirection.Length)
						{
							_secondLayerDirection[index] = new double2(forceVector.x, forceVector.y);
						}
					}
				}
			}
		}


		private float GetCellSpeed(Vector3 worldPosition)
		{
			var raycastStart = worldPosition + Vector3.up * _raycastHeight;
			var radius = Mathf.Max(_worldBounds.extents.x, _worldBounds.extents.z) / _gridResolution * _sampleWallsSphereRadius;
			if(Physics.SphereCast(raycastStart, radius, Vector3.down, out var groundHit, _raycastHeight * 2f, _groundLayerMask))
			{
				var bottomPoint = raycastStart + Vector3.down * groundHit.distance + Vector3.down * radius;
				var delta = bottomPoint.y - _walkableAltitude;
				if(delta < 0 || delta > _walkableAltitudeTolerance)
				{
					return -1f;
				}
				var speed = 1 / (1 + delta * _closeToWallsPenalty);
				if(speed > 0f && speed < 0.999f)
				{
					DebugDrawUtils.DrawWireCapsulePersistent(bottomPoint + Vector3.up * radius, 1f, radius, Color.Lerp(Color.green, Color.red, speed), 5f);
				}
				return speed;
			}

			return -1;
		}

		public void ClearSecondLayer()
		{
			if(_secondLayerDirection.IsCreated)
			{
				for(var i = 0; i < _secondLayerDirection.Length; i++)
				{
					_secondLayerDirection[i] = new double2(0, 0);
				}
			}
		}

		private void Update()
		{
			DrawDebugVisualization();
		}

		private void OnDrawGizmosSelected()
		{
			DebugDrawUtils.DrawBounds(_worldBounds, Color.green);
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
			if(_secondLayerDirection.IsCreated)
			{
				_secondLayerDirection.Dispose();
			}
		}
	}
}
