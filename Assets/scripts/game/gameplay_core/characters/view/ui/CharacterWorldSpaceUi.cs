using dream_lib.src.extensions;
using dream_lib.src.ui;
using game.gameplay_core.characters.runtime_data;
using UnityEngine;

namespace game.gameplay_core.characters.view.ui
{
	public class CharacterWorldSpaceUi : MonoBehaviour
	{
		public struct CharacterWorldSpaceUiContext
		{
			public CharacterStats CharacterStats { get; set; }
			public Transform UiPivotWorld { get; set; }
		}

		[SerializeField]
		private UiBar _healthBar;
		private CharacterWorldSpaceUiContext _context;
		private Transform _transform;

		public void Initialize(CharacterWorldSpaceUiContext context)
		{
			_context = context;
			_transform = transform;
			transform.parent = GameStaticContext.Instance.WorldToScreenUiParent.Value;
			_healthBar.SetContext(new UiBar.Context
			{
				Current = context.CharacterStats.Hp,
				Max = context.CharacterStats.MaxHp
			});
		}

		public void CustomUpdate(float deltaTime)
		{
			_healthBar.CustomUpdate();
			var screenPos = GameStaticContext.Instance.MainCamera.Value.WorldToScreenPoint(_context.UiPivotWorld.position).Round();
			_transform.position = screenPos;
		}
	}
}
