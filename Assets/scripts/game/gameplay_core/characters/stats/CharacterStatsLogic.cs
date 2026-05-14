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
		}

		public static Dictionary<StatKey, float> CalcAllStatMaxValues(CommonStatsConfig config,
			SerializableDictionary<StatKey, int> defaultValuesOverride,
			WeaponItemConfig weapon,
			SerializableDictionary<StatKey, int> statsUpgrades)
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

			if(statsUpgrades != null)
			{
				foreach(var kvp in statsUpgrades)
				{
					_cacheMaxValues[kvp.Key] += kvp.Value;
				}
			}

			//TODO: armor weight affects move speed, roll speed

			_cacheMaxValues[StatKey.Hp] += _cacheMaxValues[StatKey.Vitality] * 1;
			_cacheMaxValues[StatKey.Stamina] += _cacheMaxValues[StatKey.Endurance] * 10;

			_cacheMaxValues[StatKey.AttackDamage] = weapon.RegularAttacks[0].BaseDamage;

			foreach(var kvp in weapon.DamageScaling)
			{
				var statValue = _cacheMaxValues[kvp.Key];
				_cacheMaxValues[StatKey.AttackDamage] += statValue * kvp.Value;
			}

			return _cacheMaxValues;
		}

		public void RecalculateStats()
		{
			var data = _context.CharacterStats;

			var statsMaxValues = CalcAllStatMaxValues(GameStaticContext.Instance.CommonStatsConfig,
				_context.Config.DefaultStatsValueOverride,
				_context.Logic.InventoryLogic.RightWeapon.Config,
				GameStaticContext.Instance.PlayerSave.CharacterData.StatUpgrades
			);

			foreach(var kvp in statsMaxValues)
			{
				data.AllStats[kvp.Key].MaxValue = kvp.Value;
				data.AllStats[kvp.Key].IsHidden = data.AllStats[kvp.Key].Config.IsHidden;
			}

			data.SetStatsToMax();

			var turnSpeedMult = statsMaxValues[StatKey.TurnSpeedMultiplier];
			var moveSpeedMult = statsMaxValues[StatKey.MoveSpeedMultiplier];

			data.Locomotion.HalfTurnDurationSeconds = _context.Config.Locomotion.HalfTurnDurationSeconds / turnSpeedMult;
			data.Locomotion.HalfTurnDurationSecondsLockOn = _context.Config.Locomotion.HalfTurnDurationSecondsLockOn / turnSpeedMult;
			data.Locomotion.RunSpeed = _context.Config.Locomotion.RunSpeed * moveSpeedMult;
			data.Locomotion.WalkAcceleration = _context.Config.Locomotion.WalkAcceleration;
			data.Locomotion.WalkDeceleration = _context.Config.Locomotion.WalkDeceleration;
			data.Locomotion.WalkSpeed = _context.Config.Locomotion.WalkSpeed * moveSpeedMult;
		}
	}
}
