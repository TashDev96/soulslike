using dream_lib.src.extensions;
using UnityEngine;

namespace game.gameplay_core.characters.state_machine.states.attack
{
	public class BackstabAttackState:CriticalAttackStateBase
	{
		public BackstabAttackState(CharacterContext context, CharacterDomain target) : base(context, target)
		{
			var attackConfig = _context.RightWeapon.Value.Config.BackstabAttack;

			SetEnterParams(attackConfig);
			LockTargetInAnimation();
		}

		private void LockTargetInAnimation()
		{
			if(_target == null || _target.ExternalData.IsDead)
			{
				return;
			}

			_target.transform.rotation = Quaternion.LookRotation(( _target.transform.position - _context.Transform.Position ).SetY(0));
			_target.transform.position = _context.Transform.Position + _target.transform.forward * 1.5f;
			_target.CharacterStateMachine.LockInAnimation(_context.RightWeapon.Value.Config.BackstabbedEnemyAnimation);
		}
	}
}
