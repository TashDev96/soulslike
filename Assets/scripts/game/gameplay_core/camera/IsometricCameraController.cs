using DG.Tweening;
using dream_lib.src.camera;
using dream_lib.src.reactive;
using dream_lib.src.utils.data_types;
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

		private float _criticalAttackAnimationTimeLeft;
		private float _criticalAttackAnimationDuration;

		private Vector3 _shakeOffset;
		private Tweener _shakeTweener;

		public Camera Camera => _context.Camera.Value;

		public IsometricCameraController(Context context)
		{
			_context = context;
			switch(_context.CameraSettings.Mode)
			{
				case IsometricCameraSettings.PerspectiveMode.Orthographic:
					_context.Camera.Value.orthographic = true;
					_context.Camera.Value.orthographicSize = _context.CameraSettings.OrthoSize;
					break;
				case IsometricCameraSettings.PerspectiveMode.Perspective:
					_context.Camera.Value.orthographic = false;
					_context.Camera.Value.fieldOfView = _context.CameraSettings.FOV;
					break;
			}
		}

		public void Update(float deltaTime)
		{
			var settings = _context.CameraSettings;
			var cameraTransform = _context.Camera.Value.transform;
			var targetPosition = _context.Player.Value.ExternalData.Transform.Position;
			var forward = cameraTransform.forward;
			var altitude = _context.CameraSettings.CameraAltitude;

			if(_criticalAttackAnimationTimeLeft > 0)
			{
				_criticalAttackAnimationTimeLeft -= deltaTime;
				var normalizedTime = 1f - _criticalAttackAnimationTimeLeft / _criticalAttackAnimationDuration;
				var curve = _context.CameraSettings.CriticalAttackZoomMultiplier.Evaluate(normalizedTime);
				if(_criticalAttackAnimationTimeLeft < 0)
				{
					curve = 1;
				}
				switch(_context.CameraSettings.Mode)
				{
					case IsometricCameraSettings.PerspectiveMode.Orthographic:
						_context.Camera.Value.orthographicSize = settings.OrthoSize * curve;
						break;
					case IsometricCameraSettings.PerspectiveMode.Perspective:
						_context.Camera.Value.fieldOfView = settings.FOV * curve;
						break;
				}
			}

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

			//newPosition += cameraTransform.right * _shakeOffset.x + cameraTransform.up * _shakeOffset.y;
			newPosition += cameraTransform.up * _shakeOffset.y;
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

		public void ShowCriticalAttackAnimation(ReadOnlyTransform contextTransform, float expectedDuration)
		{
			_criticalAttackAnimationTimeLeft = expectedDuration;
			_criticalAttackAnimationDuration = expectedDuration;
		}

		public void Shake(float duration, float strength, float vertMultiplier = 1f, float horMultiplier = 1f)
		{
			_shakeTweener?.Kill();
			_shakeOffset = Vector3.zero;

			var randomOffset = Random.value * 100f;
			_shakeTweener = DOVirtual.Float(strength, 0f, duration, value =>
			{
				var seed = strength < 0.5f ? value * 20f : Time.time * 20f + randomOffset;
				_shakeOffset.x = (Mathf.PerlinNoise(seed, 0f) - 0.5f) * 2f * value * horMultiplier;
				_shakeOffset.y = (Mathf.PerlinNoise(0f, seed) - 0.5f) * 2f * value * vertMultiplier;
			}).OnComplete(() => _shakeOffset = Vector3.zero);
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
