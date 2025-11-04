using System.Collections.Generic;
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

		private readonly HashSet<string> _regenLockReasons = new();
		private readonly Dictionary<string, float> _regenMultiplierReasons = new();

		public void Initialize(Context context)
		{
			_context = context;
		}

		public bool CheckCanEnterState(CharacterStateBase stateBase)
		{
			if(stateBase.GetEnterStaminaCost() <= 0)
			{
				return true;
			}
			var staminaCost = stateBase.GetEnterStaminaCost() + stateBase.RequiredStaminaOffset;

			//here potential rpg tweaks
			return _context.Stamina.Value >= staminaCost;
		}

		public void Update(float deltaTime)
		{
			if(_context.Stamina.Value < _context.StaminaMax.Value && _regenLockReasons.Count == 0)
			{
				var baseRegenRate = deltaTime * 10f;
				var totalMultiplier = CalculateTotalRegenMultiplier();
				_context.Stamina.Value += baseRegenRate * totalMultiplier;
			}
		}

		public void SetStaminaRegenLock(string reason, bool isLocked)
		{
			if(isLocked)
			{
				_regenLockReasons.Add(reason);
			}
			else
			{
				_regenLockReasons.Remove(reason);
			}
		}

		public void SetStaminaRegenMultiplier(string reason, float multiplier)
		{
			if(multiplier <= 0f)
			{
				_regenMultiplierReasons.Remove(reason);
			}
			else
			{
				_regenMultiplierReasons[reason] = multiplier;
			}
		}

		public void RemoveStaminaRegenMultiplier(string reason)
		{
			_regenMultiplierReasons.Remove(reason);
		}

		public void SpendStamina(float amount)
		{
			_context.Stamina.Value -= amount;
		}

		public void SpendStaminaForBlock(float blockStaminaCost, out bool hadEnough)
		{
			hadEnough = _context.Stamina.Value >= blockStaminaCost;
			SpendStamina(blockStaminaCost);
		}

		public string GetDebugString()
		{
			var result = "";
			foreach(var regenLockReason in _regenLockReasons)
			{
				result += regenLockReason + "\n";
			}

			foreach(var regenMultiplierReason in _regenMultiplierReasons)
			{
				result += regenMultiplierReason.Key + " " + regenMultiplierReason.Value + "\n";
			}

			return result;
		}

		private float CalculateTotalRegenMultiplier()
		{
			var totalMultiplier = 1f;
			foreach(var multiplier in _regenMultiplierReasons.Values)
			{
				totalMultiplier *= multiplier;
			}
			return totalMultiplier;
		}
	}
}
