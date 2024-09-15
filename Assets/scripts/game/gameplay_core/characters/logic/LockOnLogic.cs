using System.Collections.Generic;
using dream_lib.src.extensions;
using dream_lib.src.reactive;
using game.gameplay_core.characters.runtime_data;
using UnityEngine;

namespace game.gameplay_core.characters.logic
{
	public class LockOnLogic
	{
		public struct Context
		{
			public Transform CharacterTransform;
			public CharacterDomain Self;
			public List<CharacterDomain> AllCharacters { get; set; }
			public MovementLogic MovementLogic { get; set; }
		}

		private readonly Context _context;

		public ReactiveProperty<CharacterDomain> LockOnTarget { get; } = new();

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

			FindLockOnTarget();
		}

		public void Update(float deltaTime)
		{
			if(!LockOnTarget.HasValue)
			{
				return;
			}

			if(LockOnTarget.Value.ExternalData.IsDead)
			{
				LockOnTarget.Value = null;
				FindLockOnTarget();
				return;
			}

			var lookVector = (LockOnTarget.Value.ExternalData.Transform.position - _context.CharacterTransform.position).SetY(0);
			_context.MovementLogic.RotateCharacter(lookVector, deltaTime);
		}

		private void FindLockOnTarget()
		{
			CharacterDomain selectedTarget = null;
			var minDistance = float.MaxValue;

			foreach(var character in _context.AllCharacters)
			{
				if(character == _context.Self || character.ExternalData.IsDead)
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
				LockOnTarget.Value = selectedTarget;
			}
		}
	}
}
