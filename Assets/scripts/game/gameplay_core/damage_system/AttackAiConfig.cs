using System;
using UnityEngine;

namespace game.gameplay_core.damage_system
{
	[Serializable]
	public class AttackAiConfig
	{
		[field: SerializeField]
		public float Range { get; set; } = 2f;
		[field: SerializeField]
		public float Sector { get; set; } = 90f;
	}
}
