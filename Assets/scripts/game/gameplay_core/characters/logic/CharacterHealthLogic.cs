using dream_lib.src.reactive;
using game.gameplay_core.characters.runtime_data;
using game.gameplay_core.characters.runtime_data.bindings;
using game.gameplay_core.damage_system;
using UnityEngine;

namespace game.gameplay_core.characters.logic
{
	public class CharacterHealthLogic
	{
		public struct Context
		{
			public ReactiveCommand<DamageInfo> ApplyDamage { get; set; }
			public CharacterStats CharacterStats { get; set; }
			public IsDead IsDead { get; set; }
		}

		private readonly Context _context;

		public CharacterHealthLogic(Context context)
		{
			_context = context;
			_context.ApplyDamage.OnExecute += ApplyDamage;
		}

		private void ApplyDamage(DamageInfo damageInfo)
		{
			_context.CharacterStats.Hp.Value -= damageInfo.DamageAmount;
			if(_context.CharacterStats.Hp.Value <= 0)
			{
				_context.IsDead.Value = true;
			}
		}
	}
}
