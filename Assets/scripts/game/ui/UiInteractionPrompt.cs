using System.Collections.Generic;
using System.Linq;
using dream_lib.src.reactive;
using dream_lib.ui.animations;
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

		[SerializeReference]
		[SerializeField]
		private List<UiAnimationBase> _appearAnims;

		private Context _context;
		private InteractionZone _selectedInteractionZone;

		private readonly HashSet<InteractionZone> _enteredInteractionZones = new();

		public void SetContext(Context context)
		{
			_context = context;

			_context.TriggersEnteredByPlayer.OnAdded += HandlePlayerEnteredTrigger;
			_context.TriggersEnteredByPlayer.OnRemoved += HandlePlayerLeftTrigger;
			gameObject.SetActive(false);

			foreach(var anim in _appearAnims)
			{
				anim.Initialize(this);
			}
		}

		private void HandlePlayerEnteredTrigger(Collider enteredTrigger)
		{
			if(enteredTrigger.TryGetComponent<InteractionZone>(out var zone))
			{
				_enteredInteractionZones.Add(zone);
				Show(zone);
			}
		}

		private void HandlePlayerLeftTrigger(Collider exitedTrigger)
		{
			if(exitedTrigger.TryGetComponent<InteractionZone>(out var zone))
			{
				RemoveInteractionZone(zone);
			}
		}

		private void RemoveInteractionZone(InteractionZone zone)
		{
			_enteredInteractionZones.Remove(zone);

			if(zone == _selectedInteractionZone)
			{
				if(_enteredInteractionZones.Count == 0)
				{
					Hide();
				}
				else
				{
					Show(_enteredInteractionZones.First());
				}
			}
		}

		private void Show(InteractionZone zone)
		{
			_selectedInteractionZone = zone;
			_text.text = _selectedInteractionZone.InteractionTextHint;
			gameObject.SetActive(true);

			foreach(var anim in _appearAnims)
			{
				anim.Play(false);
			}
		}

		private void Hide()
		{
			_selectedInteractionZone = null;

			foreach(var anim in _appearAnims)
			{
				anim.Clear(false);
			}
		}

		private void Update()
		{
			if(_selectedInteractionZone == null)
			{
				foreach(var anim in _appearAnims)
				{
					if(anim.InProgress)
					{
						return;
					}
				}
				gameObject.SetActive(false);
				return;
			}

			if(InputAdapter.GetButtonDown(InputAxesNames.Interact))
			{
				var zoneToForceRemove = _selectedInteractionZone;
				_selectedInteractionZone.InteractFromUi(_context.Player);
				if(_selectedInteractionZone == zoneToForceRemove)
				{
					RemoveInteractionZone(zoneToForceRemove);
				}
			}
		}
	}
}
