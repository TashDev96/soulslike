using dream_lib.src.reactive;
using game.gameplay_core.characters;
using game.gameplay_core.location.interactive_objects.common;
using game.input;
using TMPro;
using UnityEngine;

namespace game.ui
{
	public class UiInteractionPrompt : MonoBehaviour
	{
		public class Context
		{
			public ReactiveHashSet<Collider> TriggersEnteredByPlayer;
			public CharacterDomain Player;
		}

		[SerializeField]
		private TextMeshProUGUI _text;

		private Context _context;
		private InteractionZone _selectedInteractionZone;

		public void SetContext(Context context)
		{
			_context = context;

			_context.TriggersEnteredByPlayer.OnAdded += HandlePlayerEnteredTrigger;
			_context.TriggersEnteredByPlayer.OnRemoved += HandlePlayerLeftTrigger;
			gameObject.SetActive(false);
		}

		private void HandlePlayerEnteredTrigger(Collider obj)
		{
			if(obj.TryGetComponent<InteractionZone>(out var zone))
			{
				_selectedInteractionZone = zone;
				_text.text = _selectedInteractionZone.InteractionTextHint;
				gameObject.SetActive(true);
			}
		}

		private void HandlePlayerLeftTrigger(Collider obj)
		{
			if(_selectedInteractionZone != null && obj.gameObject == _selectedInteractionZone.gameObject)
			{
				_selectedInteractionZone = null;
				gameObject.SetActive(false);
			}
		}

		private void Update()
		{
			if(InputAdapter.GetButtonDown(InputAxesNames.Interact))
			{
				if(_selectedInteractionZone != null)
				{
					_selectedInteractionZone.InteractFromUi(_context.Player);
				}
			}
		}
	}
}
