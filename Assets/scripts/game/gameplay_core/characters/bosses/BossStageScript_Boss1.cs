using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace game.gameplay_core.characters.bosses
{
	//messy script that i don't want to refactor cuz nothing depends on it
	public class BossStageScript_Boss1 : AbstractCharacterScript
	{
		[SerializeField]
		private List<BossArmorDamageReceiver> _legArmors;

		[SerializeField]
		private float _rotationSpeedUpPhase2 = 2f;
		
		
		[SerializeField]
		private float _walkSpeedUpPhase2 = 1.5f;

		private bool _isPhase2;
		private CharacterContext _context;

		public override void SetContext(CharacterContext context)
		{
			_context = context;
		}

		private void Update()
		{
			if(_context.SelfLink == null)
			{
				return;
			}

			if(_isPhase2)
			{
				return;
			}

			if(_legArmors.Count(armor => armor.IsBroken) == _legArmors.Count)
			{
				_context.CharacterStats.Locomotion.HalfTurnDurationSeconds /= _rotationSpeedUpPhase2;
				_context.CharacterStats.Locomotion.HalfTurnDurationSecondsLockOn /= _rotationSpeedUpPhase2;
				_context.CharacterStats.Locomotion.WalkSpeed *= _walkSpeedUpPhase2;
				_isPhase2 = true;
			}
		}
	}
}
