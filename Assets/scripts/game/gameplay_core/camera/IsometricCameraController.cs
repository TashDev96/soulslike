using dream_lib.src.camera;
using dream_lib.src.reactive;
using game.gameplay_core.characters;
using UnityEngine;

namespace game.gameplay_core.camera
{
	public class IsometricCameraController : ICameraController
	{
		public struct Context
		{
			public IReadOnlyReactiveProperty<Camera> Camera;
			public ReactiveProperty<CharacterDomain> Player;
			public IsometricCameraSettings CameraSettings;
		}

		private static readonly int OcclusionSphereCenterId = Shader.PropertyToID("_OcclusionSphereCenter");
		private static readonly int OcclusionSphereRadiusId = Shader.PropertyToID("_OcclusionSphereRadius");
		private static readonly int OcclusionCircleOffsetId = Shader.PropertyToID("_OcclusionCircleOffset");

		private readonly Context _context;
		private float _currentOcclusionRadius;

		public Camera Camera => _context.Camera.Value;

		public IsometricCameraController(Context context)
		{
			_context = context;
			_context.Camera.Value.orthographic = true;
		}

		public void Update(float deltaTime)
		{
			var cameraTransform = _context.Camera.Value.transform;
			var targetPosition = _context.Player.Value.ExternalData.Transform.Position;
			var forward = cameraTransform.forward;
			var altitude = _context.CameraSettings.CameraAltitude;

			if(forward.y > -1e-3f && forward.y < 1e-3f)
			{
				var vectorToTarget = targetPosition - cameraTransform.position;
				var cameraRight = cameraTransform.right;
				var cameraUp = cameraTransform.up;
				var offsetRight = Vector3.Dot(vectorToTarget, cameraRight);
				var offsetUp = Vector3.Dot(vectorToTarget, cameraUp);
				cameraTransform.position = cameraTransform.position + cameraRight * offsetRight + cameraUp * offsetUp;
				UpdateOcclusionSphere(targetPosition, deltaTime);
				return;
			}

			var distance = -altitude / forward.y;
			var newPosition = targetPosition - forward * distance;
			cameraTransform.position = newPosition;
			UpdateOcclusionSphere(targetPosition, deltaTime);
		}

		public Vector3 ConvertScreenSpaceDirectionToWorld(Vector3 screenSpaceInput)
		{
			return Camera.ProjectScreenVectorToWorldPlaneWithSkew(screenSpaceInput);
		}

		public bool OverrideAttackDirectionOnClick(out Vector3 newDirectionWorld)
		{
			var mouseRay = Camera.ScreenPointToRay(Input.mousePosition);
			var playerPos = _context.Player.Value.Context.Transform.Position;
			var plane = new Plane(Vector3.up, playerPos);

			if(plane.Raycast(mouseRay, out var distance))
			{
				var hitPoint = mouseRay.GetPoint(distance);
				var direction = hitPoint - playerPos;
				direction.y = 0;
				if(direction.sqrMagnitude > 0.01f)
				{
					newDirectionWorld = direction.normalized;
					return true;
				}
			}
			newDirectionWorld = default;
			return false;
		}

		private void UpdateOcclusionSphere(Vector3 targetPosition, float deltaTime)
		{
			var settings = _context.CameraSettings;
			var center = targetPosition + Vector3.up * settings.OcclusionSphereHeightOffset;
			var cameraPos = _context.Camera.Value.transform.position;
			var toCenter = center - cameraPos;
			var distance = toCenter.magnitude;
			var viewDir = toCenter / distance;

			var isOccluded = Physics.SphereCast(
				cameraPos,
				settings.OcclusionCheckRadius,
				viewDir,
				out _,
				distance,
				settings.OcclusionCheckLayerMask
			);

			var targetRadius = isOccluded ? settings.OcclusionSphereRadius : 0f;
			_currentOcclusionRadius = Mathf.MoveTowards(_currentOcclusionRadius, targetRadius, settings.OcclusionFadeSpeed * deltaTime);

			Shader.SetGlobalVector(OcclusionSphereCenterId, new Vector4(center.x, center.y, center.z, 1));
			Shader.SetGlobalFloat(OcclusionSphereRadiusId, _currentOcclusionRadius);
			Shader.SetGlobalFloat(OcclusionCircleOffsetId, settings.OcclusionCircleOffset);

			CameraSettings.GizmoCircleCenter = center - viewDir * settings.OcclusionCircleOffset;
			CameraSettings.GizmoCircleNormal = viewDir;
			CameraSettings.GizmoCircleRadius = _currentOcclusionRadius;
		}
	}
}
