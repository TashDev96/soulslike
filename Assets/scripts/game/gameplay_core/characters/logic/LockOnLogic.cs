using System.Collections.Generic;
using dream_lib.src.extensions;
using dream_lib.src.reactive;

namespace game.gameplay_core.characters.logic
{
	public class LockOnLogic
	{
		private CharacterContext _context;

		public ReactiveProperty<CharacterDomain> LockOnTarget { get; } = new();
		public bool IsLockedOn => LockOnTarget.HasValue;
		public bool DisableRotationForThisFrame { get; set; }
 

		public void SetContext(CharacterContext context)
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
				var lookVector = (LockOnTarget.Value.ExternalData.Transform.Position - _context.Transform.Position).SetY(0);
				_context.MovementLogic.RotateCharacter(lookVector, _context.Config.Locomotion.HalfTurnDurationSecondsLockOn, deltaTime);
			}

			DisableRotationForThisFrame = false;
		}

		private void FindLockOnTarget()
		{
			CharacterDomain selectedTarget = null;
			var minDistance = float.MaxValue;
			var maxDistance = 30f;

			foreach(var character in LocationStaticContext.Instance.Characters)
			{
				if(character == _context.SelfLink || character.ExternalData.IsDead)
				{
					continue;
				}

				var distance = (_context.Transform.Position - character.ExternalData.Transform.Position).sqrMagnitude;
				if(distance < minDistance && distance < maxDistance * maxDistance)
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
