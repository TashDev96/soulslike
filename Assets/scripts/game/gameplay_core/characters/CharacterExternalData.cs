using game.gameplay_core.characters.view;
using UnityEngine;

namespace game.gameplay_core.characters
{
	public class CharacterExternalData
	{
		private readonly CharacterContext _context;

		public string Id => _context.CharacterId.Value;
		public Transform Transform => _context.Transform;
		public LockOnTargetView[] LockOnTargets => _context.LockOnTargets;
		public bool IsDead => _context.IsDead.Value;

		public CharacterExternalData(CharacterContext context)
		{
			_context = context;
		}
	}
}
