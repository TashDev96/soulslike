using dream_lib.src.extensions;
using dream_lib.src.reactive;
using game.gameplay_core.location;
using UnityEngine;

namespace game.gameplay_core.characters.logic
{
	public class LockOnLogic
	{
		private CharacterContext _context;

		public ReactiveProperty<CharacterDomain> LockOnTarget { get; } = new();
		public bool IsLockedOn => LockOnTarget.HasValue;
		public bool DisableRotationForThisFrame { get; set; }

		private int _obstacleMask;

		public void SetContext(CharacterContext context)
		{
			_context = context;
			_obstacleMask = LayerMask.GetMask("Default", "LevelGeometry", "Doors");
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

		public void HandleLockOnSelectedByAI(CharacterDomain target)
		{
			LockOnTarget.Value = target;
		}

		public void Update(float deltaTime)
		{
			if(!LockOnTarget.HasValue || _context.IsDead.Value || _context.IsBirdMode.Value)
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
				_context.Logic.MovementLogic.RotateCharacter(lookVector, _context.CharacterStats.Locomotion.HalfTurnDurationSecondsLockOn, deltaTime);
			}

			DisableRotationForThisFrame = false;
		}

		public void Reset()
		{
			LockOnTarget.Value = null;
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

				var allPointBlocked = true;
				foreach(var point in character.ExternalData.LockOnPoints)
				{
					var from = _context.Transform.Position + Vector3.up;
					var to = point.transform.position;
					if(!Physics.Linecast(from, to, _obstacleMask))
					{
						allPointBlocked = false;
						break;
					}
				}

				if(allPointBlocked)
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
