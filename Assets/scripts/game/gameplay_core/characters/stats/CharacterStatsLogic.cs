using System.Collections.Generic;
using dream_lib.src.utils.data_types;
using game.gameplay_core.characters.stats.config;
using game.gameplay_core.inventory.item_configs;

namespace game.gameplay_core.characters.stats
{
	public class CharacterStatsLogic
	{
		private static Dictionary<StatKey, float> _cacheMaxValues;
		private CharacterContext _context;

		public void SetContext(CharacterContext context)
		{
			_context = context;
			RecalculateStats();
		}

		public static Dictionary<StatKey, float> CalcAllStatMaxValues(CommonStatsConfig config, SerializableDictionary<StatKey, int> defaultValuesOverride, WeaponItemConfig weapon)
		{
			if(_cacheMaxValues == null)
			{
				_cacheMaxValues = new Dictionary<StatKey, float>();
			}

			var keys = config.Stats.Keys;
			foreach(var statKey in keys)
			{
				_cacheMaxValues[statKey] = config.Stats[statKey].DefaultValue;
				if(defaultValuesOverride.TryGetValue(statKey, out var overrideBaseValue))
				{
					_cacheMaxValues[statKey] = overrideBaseValue;
				}
			}

			_cacheMaxValues[StatKey.Hp] += _cacheMaxValues[StatKey.Vitality] * 1;
			_cacheMaxValues[StatKey.Stamina] += _cacheMaxValues[StatKey.Endurance] * 10;

			_cacheMaxValues[StatKey.AttackDamage] = weapon.RegularAttacks[0].BaseDamage;

			return _cacheMaxValues;
		}

		private void RecalculateStats()
		{
			var data = _context.CharacterStats;

			//TODO calculate max values
			data.Hp.SetToMax();
			data.Stamina.SetToMax();
			data.Poise.SetToMax();

			data.Locomotion.HalfTurnDurationSeconds = _context.Config.Locomotion.HalfTurnDurationSeconds;
			data.Locomotion.HalfTurnDurationSecondsLockOn = _context.Config.Locomotion.HalfTurnDurationSecondsLockOn;
			data.Locomotion.RunSpeed = _context.Config.Locomotion.RunSpeed;
			data.Locomotion.WalkAcceleration = _context.Config.Locomotion.WalkAcceleration;
			data.Locomotion.WalkDeceleration = _context.Config.Locomotion.WalkDeceleration;
			data.Locomotion.WalkSpeed = _context.Config.Locomotion.WalkSpeed;
		}

		//TODO: solid stats increment logic, with multipliers paired with string ids or smh
	}
}
