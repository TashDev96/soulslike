using dream_lib.src.reactive;
using game.gameplay_core.characters.logic;
using game.gameplay_core.characters.runtime_data.bindings;
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
	}
}
