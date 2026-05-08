using dream_lib.src.extensions;
using dream_lib.src.reactive;
using dream_lib.ui;
using UnityEngine;

namespace game.gameplay_core.characters.view.ui
{
	public class CharacterWorldSpaceUi : MonoBehaviour, ICharacterWorldSpaceUi
	{
		public struct CharacterWorldSpaceUiContext
		{
			public CharacterContext CharacterContext;
			public ReactiveCommand<float> LocationUiUpdate;
			public Transform UiPivotWorld { get; set; }
		}

		[SerializeField]
		private UiBar _healthBar;
		[SerializeField]
		private CharacterDamageCounterUi _damageCounter;

		private CharacterWorldSpaceUiContext _context;
		private Transform _transform;

		public void Initialize(CharacterWorldSpaceUiContext context)
		{
			_context = context;
			_transform = transform;
			transform.parent = GameStaticContext.Instance.WorldToScreenUiParent.Value;
			_healthBar.SetContext(new UiBar.Context
			{
				Current = context.CharacterContext.CharacterStats.Hp.Current,
				Max = context.CharacterContext.CharacterStats.Hp.Max,
				RecoverableAmount = context.CharacterContext.CharacterStats.Hp.Recoverable,
				CustomUpdate = context.LocationUiUpdate
			});

			_damageCounter.SetContext(context.CharacterContext);

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
