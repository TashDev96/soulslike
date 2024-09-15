using System.Collections.Generic;
using dream_lib.src.reactive;
using UnityEngine;

namespace game.gameplay_core.characters.logic
{
	public class LockOnLogic
	{
		public struct Context
		{
			public Transform CharacterTransform;
			public List<CharacterDomain> AllCharacters { get; set; }
			public CharacterDomain Self;
		}

		private readonly Context _context;

		public ReactiveProperty<Transform> LockOnTarget { get; private set; } = new();

		public LockOnLogic(Context context)
		{
			_context = context;
		}

		public void HandleLockOnTriggerInput()
		{
			if(LockOnTarget.HasValue)
			{
				LockOnTarget.Value = null;
				return;
			}

			CharacterDomain selectedTarget = null;
			float minDistance = float.MaxValue;

			foreach(var character in _context.AllCharacters)
			{
				if(character == _context.Self)
				{
					continue;
				}

				var distance = (_context.CharacterTransform.position - character.ExternalData.Transform.position).sqrMagnitude;
				if(distance < minDistance)
				{
					minDistance = distance;
					selectedTarget = character;
				}
			}

			if(selectedTarget != null)
			{
				LockOnTarget.Value = selectedTarget.ExternalData.Transform;
			}
		}
	}
}
