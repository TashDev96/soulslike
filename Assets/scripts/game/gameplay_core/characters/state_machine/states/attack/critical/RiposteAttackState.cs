using dream_lib.src.extensions;
using game.enums;
using game.gameplay_core.damage_system;
using UnityEngine;

namespace game.gameplay_core.characters.state_machine.states.attack.critical
{
	public class RiposteAttackState : CriticalAttackStateBase
	{
		private readonly WeaponView _weaponView;

		public RiposteAttackState(CharacterContext context, CharacterDomain target) : base(context, target)
		{
			_weaponView = _context.EquippedWeaponViews[EquipmentSlotType.RightHand];
			var attackConfig = _weaponView.Config.RiposteAttack;

			SetEnterParams(attackConfig);
			LockTargetInAnimation();
		}

		private void LockTargetInAnimation()
		{
			if(_target == null || _target.ExternalData.IsDead)
			{
				return;
			}

			_target.transform.rotation = Quaternion.LookRotation((_context.Transform.Position - _target.transform.position).SetY(0));
			_target.transform.position = _context.Transform.Position - _target.transform.forward * 1.5f;
			_target.CharacterStateMachine.LockInAnimation(_target.Config.RipostedAnimation);
		}
	}
}
