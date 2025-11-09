#if UNITY_EDITOR

using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace game.gameplay_core.debug
{
	public class GravityDebugSimulator : MonoBehaviour
	{
		[SerializeField]
		private Transform _customGravityObject;

		[SerializeField]
		private Transform _projectGravityObject;

		[SerializeField]
		private float _startSpeed;

		[SerializeField]
		private Vector3 _customGravity = new(0, -9.81f, 0);

		[SerializeField]
		private float _timeout = 10f;

		[SerializeField]
		private float _groundCheckDistance = 0.1f;

		[SerializeField]
		private LayerMask _groundLayerMask = -1;

		[SerializeField]
		private float _velocityLineScale = 1f;

		[SerializeField]
		private bool _restartBothOnGroundHit;

		[SerializeField]
		private float _airDamping;

		private Vector3 _customGravityVelocity;
		private Vector3 _projectGravityVelocity;
		private float _elapsedTime;
		private bool _isSimulating;
		private float _customGravityFallStartTime;
		private float _projectGravityFallStartTime;

		private Vector3 StartVelocity => transform.forward * _startSpeed;

		private void Awake()
		{
			if(_customGravityObject == null && transform.childCount > 0)
			{
				_customGravityObject = transform.GetChild(0);
			}

			if(_projectGravityObject == null && transform.childCount > 1)
			{
				_projectGravityObject = transform.GetChild(1);
			}

			ResetSimulation();
		}

		private void Update()
		{
			if(!_isSimulating)
			{
				return;
			}

			_elapsedTime += Time.deltaTime;

			if(_elapsedTime >= _timeout)
			{
				ResetSimulation();
				return;
			}

			var customHitGround = UpdateSimulation(_customGravityObject, ref _customGravityVelocity, _customGravity, transform.position, ref _customGravityFallStartTime, "Custom Gravity");
			var projectHitGround = UpdateSimulation(_projectGravityObject, ref _projectGravityVelocity, Physics.gravity, transform.position, ref _projectGravityFallStartTime, "Project Gravity");

			if(_restartBothOnGroundHit && (customHitGround || projectHitGround))
			{
				ResetSimulation();
			}
		}

		private bool UpdateSimulation(Transform simObject, ref Vector3 velocity, Vector3 gravity, Vector3 startPosition, ref float fallStartTime, string simulationName)
		{
			if(simObject == null)
			{
				return false;
			}

			if(fallStartTime < 0f)
			{
				fallStartTime = Time.time;
			}

			if(_airDamping > 0f && velocity.sqrMagnitude > 0.0001f)
			{
				var velocityMagnitude = velocity.magnitude;
				var dampingForce = -velocity.normalized * velocityMagnitude * _airDamping;
				velocity += dampingForce * Time.deltaTime;
			}

			velocity += gravity * Time.deltaTime;
			var movement = velocity * Time.deltaTime;
			var newPosition = simObject.position + movement;

			if(CheckGroundHit(simObject.position, newPosition, out var hitPoint))
			{
				var fallDuration = Time.time - fallStartTime;
				Debug.Log($"{simulationName} fall duration: {fallDuration:F3}s");
				simObject.position = hitPoint;
				velocity = StartVelocity;
				simObject.position = startPosition;
				fallStartTime = -1f;
				return true;
			}

			simObject.position = newPosition;
			return false;
		}

		private bool CheckGroundHit(Vector3 from, Vector3 to, out Vector3 hitPoint)
		{
			hitPoint = to;

			var direction = (to - from).normalized;
			var distance = Vector3.Distance(from, to);

			if(Physics.Raycast(from, direction, out var hit, distance + _groundCheckDistance, _groundLayerMask))
			{
				if(hit.distance <= distance)
				{
					hitPoint = hit.point;
					return true;
				}
			}

			if(Physics.Raycast(to, Vector3.down, out hit, _groundCheckDistance, _groundLayerMask))
			{
				hitPoint = hit.point;
				return true;
			}

			return false;
		}

		private void ResetSimulation()
		{
			_elapsedTime = 0f;
			_isSimulating = true;

			var startVel = StartVelocity;

			if(_customGravityObject != null)
			{
				_customGravityVelocity = startVel;
				_customGravityObject.position = transform.position;
				_customGravityFallStartTime = -1f;
			}

			if(_projectGravityObject != null)
			{
				_projectGravityVelocity = startVel;
				_projectGravityObject.position = transform.position;
				_projectGravityFallStartTime = -1f;
			}
		}

		[Button("Set Gravity to Project Values")]
		private void SetGravityToProjectValues()
		{
			_customGravity = Physics.gravity;
		}

#if UNITY_EDITOR
		[Button("Save Gravity to Project Settings")]
		private void SaveGravityToProjectSettings()
		{
			Physics.gravity = _customGravity;
			var physicsManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/DynamicsManager.asset")[0];
			var serializedObject = new SerializedObject(physicsManager);
			var gravityProperty = serializedObject.FindProperty("m_Gravity");
			gravityProperty.vector3Value = _customGravity;
			serializedObject.ApplyModifiedProperties();
			AssetDatabase.SaveAssets();
			Debug.Log($"Gravity saved to project settings: {_customGravity}");
		}
#endif

		private void OnValidate()
		{
			if(Application.isPlaying && _isSimulating)
			{
				ResetSimulation();
			}
		}

		private void OnDrawGizmosSelected()
		{
			if(_customGravityObject != null && _startSpeed > 0.001f)
			{
				var startPos = transform.position;
				var endPos = startPos + transform.forward * _velocityLineScale;
				Gizmos.color = Color.green;
				Gizmos.DrawLine(startPos, endPos);
			}

			if(_projectGravityObject != null && _startSpeed > 0.001f)
			{
				var startPos = transform.position;
				var endPos = startPos + transform.forward * _velocityLineScale;
				Gizmos.color = Color.cyan;
				Gizmos.DrawLine(startPos, endPos);
			}
		}
	}
}

#endif
