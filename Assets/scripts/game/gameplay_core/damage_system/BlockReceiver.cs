using dream_lib.src.reactive;
using game.gameplay_core.characters;
using game.gameplay_core.characters.logic;
using game.gameplay_core.characters.state_machine.states;
using UnityEngine;

namespace game.gameplay_core.damage_system
{
	public class BlockReceiver : MonoBehaviour
	{
		public struct Context
		{
			public IReadOnlyReactiveProperty<Team> Team { get; set; }
			public IReadOnlyReactiveProperty<string> CharacterId { get; set; }
			public WeaponConfig WeaponConfig { get; set; }
			public BlockLogic BlockLogic { get; set; }
			public IReadOnlyReactiveProperty<CharacterStateBase> CurrentState { get; set; }
			public ReactiveCommand<CharacterDomain> OnParryTriggered { get; set; }
		}

		private Context _context;

		public Team OwnerTeam => _context.Team.Value;
		public string CharacterId => _context.CharacterId.Value;

		public void Initialize(Context context)
		{
			_context = context;
			gameObject.SetActive(false);
		}

		public void ApplyDamage(DamageInfo damageInfo, out bool deflectAttack)
		{
			_context.BlockLogic.ResolveBlock(damageInfo, _context.WeaponConfig, out deflectAttack);
		}

		public bool TryResolveParry(CharacterDomain damageDealer)
		{
			if(_context.CurrentState.Value is ParryState parryState)
			{
				if(parryState.IsInActiveFrames)
				{
					damageDealer.CharacterStateMachine.TriggerParryStun();
					_context.OnParryTriggered.Execute(damageDealer);
					return true;
				}
			}
			return false;
		}
	}
}
