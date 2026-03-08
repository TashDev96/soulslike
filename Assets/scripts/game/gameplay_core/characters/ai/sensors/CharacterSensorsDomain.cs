using System.Collections.Generic;
using dream_lib.src.utils.drawers;
using UnityEngine;

namespace game.gameplay_core.characters.ai.sensors
{
	public class CharacterSensorsDomain : MonoBehaviour
	{
		[SerializeField]
		private bool _drawEyes;
		[SerializeField]
		private bool _drawEars;
		[SerializeField]
		private bool _drawObservations;
		[SerializeField]
		private List<EyeSensor> _eyes = new();
		[SerializeField]
		private List<EarSensor> _ears = new();

		[SerializeField]
		private bool _drawVisualPoints;
		[SerializeField]
		private List<VisualInfoPoint> _visualInfoPoints = new();
		private CharacterDomain _characterDomain;

		private readonly List<CharacterObservation> _characterObservations = new();

		public IReadOnlyCollection<CharacterObservation> CharacterObservations => _characterObservations;

		public void Initialize(CharacterDomain characterDomain)
		{
			_characterDomain = characterDomain;

			foreach(var visualInfoPoint in _visualInfoPoints)
			{
				visualInfoPoint.Initialize(characterDomain);
			}

			foreach(var eye in _eyes)
			{
				eye.Initialize(_characterObservations);
			}

			foreach(var ear in _ears)
			{
				ear.Initialize(_characterObservations);
			}
		}

		public void CustomUpdate(float deltaTime)
		{
			foreach(var eye in _eyes)
			{
				eye.UpdateExistingObservations(deltaTime);
				eye.Observe();
			}

			if(_drawObservations)
			{
				foreach(var characterVisualObservation in _characterObservations)
				{
					Debug.DrawLine(_characterDomain.transform.position + Vector3.up, characterVisualObservation.Position + Vector3.up, Color.green, 0.1f, false);
				}
			}
		}

		private void OnDrawGizmosSelected()
		{
			if(_drawVisualPoints)
			{
				foreach(var visualInfoPoint in _visualInfoPoints)
				{
					DebugDrawUtils.DrawCross(visualInfoPoint.Position, 0.1f, Color.red);
				}
			}

			if(_drawEyes)
			{
				foreach(var eye in _eyes)
				{
					eye.DrawGizmosSelected();
				}
			}

			if(_drawEars)
			{
				foreach(var ear in _ears)
				{
					ear.DrawGizmosSelected();
				}
			}
		}
	}
}
