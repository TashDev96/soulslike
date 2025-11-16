using dream_lib.src.reactive;
using dream_lib.src.utils.data_types;
using game.gameplay_core.characters.config;
using game.gameplay_core.characters.runtime_data;
using game.gameplay_core.characters.runtime_data.bindings;
using game.gameplay_core.characters.view;
using game.gameplay_core.damage_system;
using UnityEngine;

namespace game.gameplay_core.characters
{
	public class CharacterExternalData
	{
		private readonly CharacterContext _context;

		public string Id => _context.CharacterId.Value;
		public ReadOnlyTransform Transform => _context.Transform;
		public LockOnTargetView[] LockOnTargets => _context.LockOnTargets;
		public bool IsDead => _context.IsDead.Value;
		public Team Team => _context.Team.Value;

		public CharacterStats Stats => _context.CharacterStats;
		public CharacterConfig Config => _context.Config;
		public ApplyDamageCommand ApplyDamage => _context.ApplyDamage;
		
		public ReactiveHashSet<Collider> EnteredTriggers => _context.EnteredTriggers;

		public CharacterExternalData(CharacterContext context)
		{
			_context = context;
		}
	}
}
