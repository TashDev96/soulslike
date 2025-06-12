using dream_lib.src.reactive;
using game.gameplay_core.characters.config;
using game.gameplay_core.characters.runtime_data.bindings.stats;
using game.gameplay_core.characters.state_machine.states;
using game.gameplay_core.damage_system;

namespace game.gameplay_core.characters.logic
{
	public class StaminaLogic
	{
		public struct Context
		{
			public Stamina Stamina;
			public StaminaMax StaminaMax;
			public CharacterConfig CharacterConfig;
			public ReactiveProperty<WeaponView> CurrentWeapon;
		}

		private Context _context;

		public void Initialize(Context context)
		{
			_context = context;
		}

		public bool CheckCanEnterState(CharacterStateBase stateBase)
		{
			var staminaCost = stateBase.GetEnterStaminaCost() + stateBase.RequiredStaminaOffset;

			//here potential rpg tweaks
			return _context.Stamina.Value >= staminaCost;
		}

		public void Update(float deltaTime)
		{
			if(_context.Stamina.Value < _context.StaminaMax.Value)
			{
				_context.Stamina.Value += deltaTime;
			}
		}

		public void SpendStamina(float staminaCostPerSecond)
		{
			_context.Stamina.Value -= staminaCostPerSecond;
		}

		public void SpendStaminaForStateEnter(CharacterStateBase newState)
		{
			SpendStamina(newState.GetEnterStaminaCost());
		}
	}
}
