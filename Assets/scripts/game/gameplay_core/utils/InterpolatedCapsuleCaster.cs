using System.Collections.Generic;
using UnityEngine;

namespace game.gameplay_core.utils
{
	public class InterpolatedCapsuleCaster
	{
		private const float MaxPosStep = 0.2f;
		private const float MaxAngleStep = 20f;

		private List<CapsuleCaster> _colliders;

		private int _currentStep;
		private int _stepsCount;

		public bool Terminated { get; private set; }

		public void Start(List<CapsuleCaster> colliders)
		{
			Terminated = false;
			_colliders = colliders;
			_currentStep = 0;
			_stepsCount = 0;

			for(var i = 0; i < _colliders.Count; i++)
			{
				var colliderTransform = _colliders[i].transform;
				var prevRotation = Quaternion.Euler(_colliders[i].PreviousTransform.EulerAngles);
				var currentRotation = colliderTransform.rotation;
				var prevPosition = _colliders[i].PreviousTransform.Position;
				var currentPosition = colliderTransform.position;

				_colliders[i].NextTransform.Set(colliderTransform);

				var angleDiff = Quaternion.Angle(prevRotation, currentRotation);
				var posDiff = (currentPosition - prevPosition).magnitude;

				var colliderSteps = Mathf.CeilToInt(Mathf.Max(angleDiff / MaxAngleStep, posDiff / MaxPosStep));
				if(colliderSteps > _stepsCount)
				{
					_stepsCount = colliderSteps;
				}

				_colliders[i].transform.position = prevPosition;
				_colliders[i].transform.rotation = prevRotation;
				_colliders[i].UpdateMovementDirectionCache();
			}
		}

		public bool MoveNext()
		{
			_currentStep++;

			if(Terminated || _currentStep > _stepsCount)
			{
				return false;
			}

			var interpolationVal = (float)_currentStep / _stepsCount;

			for(var i = 0; i < _colliders.Count; i++)
			{
				var collider = _colliders[i];
				var colliderTransform = collider.transform;

				var prevRotation = Quaternion.Euler(collider.PreviousTransform.EulerAngles);
				var nextRotation = Quaternion.Euler(collider.NextTransform.EulerAngles);
				var prevPosition = collider.PreviousTransform.Position;
				var nextPosition = collider.NextTransform.Position;

				colliderTransform.position = Vector3.Lerp(prevPosition, nextPosition, interpolationVal);
				colliderTransform.rotation = Quaternion.Lerp(prevRotation, nextRotation, interpolationVal);
				collider.UpdateMovementDirectionCache(true, _currentStep);
			}

			return true;
		}

		public IEnumerable<CapsuleCaster> GetActiveColliders()
		{
			for(var i = 0; i < _colliders.Count; i++)
			{
				yield return _colliders[i];
			}
		}

		public void ResetOnInterrupted()
		{
			Terminated = true;
			foreach(var collider in _colliders)
			{
				collider.transform.position = collider.NextTransform.Position;
				collider.transform.rotation = Quaternion.Euler(collider.NextTransform.EulerAngles);
			}
		}
	}
}
