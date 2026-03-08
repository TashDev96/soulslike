using System;
using System.Collections.Generic;
using dream_lib.src.utils.drawers;
using UnityEngine;

namespace game.gameplay_core.characters.ai.sensors
{
	[Serializable]
	public class EarSensor
	{
		[SerializeField]
		private float _sensitivityMultiplier = 1f;

		[SerializeField]
		private Transform _rootTransform;
		[SerializeField]
		private Vector3 _localPosition;

		private List<CharacterObservation> _characterObservations;

		public Vector3 Position => _rootTransform.TransformPoint(_localPosition);

		public void Initialize(List<CharacterObservation> characterObservations)
		{
			_characterObservations = characterObservations;
			LocationStaticContext.Instance.WorldInfo.PropagateSoundInfo.OnExecute += HandleSoundStart;
		}

		private void HandleSoundStart(SoundInfo sound)
		{
			var dist = (sound.Position - Position).magnitude;

			if(dist > sound.NormalHearDistance + _sensitivityMultiplier)
			{
				return;
			}

			if(sound.Character != null)
			{
				var characterObnservationExists = false;
				foreach(var characterObservation in _characterObservations)
				{
					if(characterObservation.Character == sound.Character)
					{
						characterObservation.Position = sound.Position;
						characterObservation.TimePassed = 0;
						characterObnservationExists = true;
						break;
					}
				}

				if(!characterObnservationExists)
				{
					_characterObservations.Add(new CharacterObservation
					{
						Character = sound.Character,
						Position = sound.Position,
						TimePassed = 0
					});
				}
			}
		}

		public void DrawGizmosSelected()
		{
			DebugDrawUtils.DrawWireCircle(Position, _sensitivityMultiplier, Vector3.up, Color.darkGreen);
		}
	}
}
