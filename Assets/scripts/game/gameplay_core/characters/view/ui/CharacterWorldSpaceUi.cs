using dream_lib.src.extensions;
using dream_lib.src.reactive;
using dream_lib.src.ui;
using game.gameplay_core.characters.runtime_data;
using game.gameplay_core.characters.runtime_data.bindings;
using game.gameplay_core.characters;
using UnityEngine;

namespace game.gameplay_core.characters.view.ui
{
	public class CharacterWorldSpaceUi : MonoBehaviour
	{
		public struct CharacterWorldSpaceUiContext
		{
			public CharacterContext CharacterContext;
			public ReactiveCommand<float> LocationUiUpdate;
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
				Current = context.CharacterContext.CharacterStats.Hp,
				Max = context.CharacterContext.CharacterStats.HpMax,
				CustomUpdate = context.LocationUiUpdate
			});

			context.LocationUiUpdate.OnExecute += CustomUpdate;
		}

		public void Reset()
		{
			_healthBar.Reset();
		}

		private void CustomUpdate(float deltaTime)
		{
			var screenPos = GameStaticContext.Instance.MainCamera.Value.WorldToScreenPoint(_context.UiPivotWorld.position).Round();
			_transform.position = screenPos;
		}
	}
}
