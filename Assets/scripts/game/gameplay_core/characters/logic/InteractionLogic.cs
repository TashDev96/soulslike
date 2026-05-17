using System.Collections.Generic;
using dream_lib.src.reactive;
using game.gameplay_core.location.interactive_objects.common;
using UnityEngine;

namespace game.gameplay_core.characters.logic
{
	public class InteractionLogic
	{
		public ReactiveCommand<NotificationInfo> OnNotificationAdded = new();
		public ReactiveCommand OnNotificationsCleared = new();

		private readonly List<InteractionZone> _enteredInteractionZones = new();
		private CharacterContext _context;

		private bool _isBlockedByNotification;

		public ReactiveProperty<string> InteractionText { get; } = new();

		private InteractionZone SelectedInteractionZone => _enteredInteractionZones.Count > 0 ? _enteredInteractionZones[0] : null;

		public void SetContext(CharacterContext context)
		{
			_context = context;

			_context.EnteredTriggers.OnAdded += HandleEnteredTrigger;
			_context.EnteredTriggers.OnRemoved += HandleLeftTrigger;
		}

		public void TriggerCurrentInteraction()
		{
			if(_isBlockedByNotification)
			{
				_isBlockedByNotification = false;
				InteractionText.Value = SelectedInteractionZone != null ? SelectedInteractionZone.InteractionTextHint : null;
				OnNotificationsCleared.Execute();
				return;
			}

			if(SelectedInteractionZone != null)
			{
				var zoneToInteract = SelectedInteractionZone;
				RemoveInteractionZone(zoneToInteract);
				zoneToInteract.InteractFromUi(_context.SelfLink);
			}
		}

		public void AddNotification(string message, Sprite icon = null)
		{
			if(!_context.IsPlayer.Value)
			{
				return;
			}
			_isBlockedByNotification = true;
			InteractionText.Value = "Ok";
			OnNotificationAdded.Execute(new NotificationInfo
			{
				Message = message,
				Icon = icon
			});
		}

		private void HandleEnteredTrigger(Collider enteredTrigger)
		{
			if(enteredTrigger.TryGetComponent<InteractionZone>(out var zone))
			{
				if(_enteredInteractionZones.Contains(zone))
				{
					_enteredInteractionZones.Remove(zone);
				}
				_enteredInteractionZones.Insert(0, zone);
				SetZoneToInteractWith(zone);
			}
		}

		private void SetZoneToInteractWith(InteractionZone zone)
		{
			if(_isBlockedByNotification)
			{
				return;
			}

			if(zone == null)
			{
				InteractionText.Value = null;
				return;
			}
			InteractionText.Value = SelectedInteractionZone.InteractionTextHint;
		}

		private void HandleLeftTrigger(Collider exitedTrigger)
		{
			if(exitedTrigger.TryGetComponent<InteractionZone>(out var zone))
			{
				RemoveInteractionZone(zone);
			}
		}

		private void RemoveInteractionZone(InteractionZone zone)
		{
			_enteredInteractionZones.Remove(zone);

			SetZoneToInteractWith(SelectedInteractionZone);
		}
	}

	public class NotificationInfo
	{
		public string Message;
		public Sprite Icon;
	}

	//какие типы интерактивности есть в игре?
	//1) интерактив зона (предмет, дверь итд)
	//2) скликнуть уведомление (открывается с другой стороны, предмет получен итд)
}
