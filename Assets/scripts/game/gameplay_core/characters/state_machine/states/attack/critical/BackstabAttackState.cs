using dream_lib.src.extensions;
using game.enums;
using game.gameplay_core.damage_system;
using UnityEngine;

namespace game.gameplay_core.characters.state_machine.states.attack.critical
{
	public class BackstabAttackState : CriticalAttackStateBase
	{
		private readonly WeaponView _weaponView;

		public BackstabAttackState(CharacterContext context, CharacterDomain target) : base(context, target)
		{
			_weaponView = _context.EquippedWeaponViews[EquipmentSlotType.RightHand];

			var attackConfig = _weaponView.Config.BackstabAttack;

			SetEnterParams(attackConfig);
			LockTargetInAnimation();
		}

		private void LockTargetInAnimation()
		{
			if(_target == null || _target.ExternalData.IsDead)
			{
				return;
			}

			_target.transform.rotation = Quaternion.LookRotation((_target.transform.position - _context.Transform.Position).SetY(0));
			_target.transform.position = _context.Transform.Position + _target.transform.forward * 1.5f;
			_target.CharacterStateMachine.LockInAnimation(_weaponView.Config.BackstabbedEnemyAnimation);
		}
	}
}
