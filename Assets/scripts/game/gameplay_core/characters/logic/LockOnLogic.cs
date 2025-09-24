using System.Collections.Generic;
using dream_lib.src.extensions;
using dream_lib.src.reactive;
using dream_lib.src.utils.data_types;
using game.gameplay_core.characters.runtime_data.bindings;

namespace game.gameplay_core.characters.logic
{
	public class LockOnLogic
	{
		public struct Context
		{
			public ReadOnlyTransform CharacterTransform;
			public CharacterDomain Self;
			public List<CharacterDomain> AllCharacters { get; set; }
			public MovementLogic MovementLogic { get; set; }
			public IsDead IsDead { get; set; }
		}

		private readonly Context _context;

		public ReactiveProperty<CharacterDomain> LockOnTarget { get; } = new();
		public bool IsLockedOn => LockOnTarget.HasValue;
		public bool DisableRotationForThisFrame { get; set; }
		public List<CharacterDomain> AllCharacters => _context.AllCharacters;

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
			if(!LockOnTarget.HasValue || _context.IsDead.Value)
			{
				return;
			}

			if(LockOnTarget.Value.ExternalData.IsDead)
			{
				LockOnTarget.Value = null;
				FindLockOnTarget();
				return;
			}

			if(!DisableRotationForThisFrame)
			{
				var lookVector = (LockOnTarget.Value.ExternalData.Transform.Position - _context.CharacterTransform.Position).SetY(0);
				_context.MovementLogic.RotateCharacter(lookVector, deltaTime);
			}

			DisableRotationForThisFrame = false;
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

				var distance = (_context.CharacterTransform.Position - character.ExternalData.Transform.Position).sqrMagnitude;
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
