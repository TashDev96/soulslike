using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace game.gameplay_core.location.view
{
	[Serializable]
	public class LootConfigurableView
	{
		public List<LootConfig> LootConfigs;
	}

	[Serializable]
	public class LootConfig
	{
		[ValueDropdown("@AddressableAssetNames.ItemConfigs")]
		public string ConfigId;

		[ValueDropdown("@AddressableAssetNames.LootVfxPrefabs")]
		public string LootVfxPrefab;

		public float MaxFlightDistance = 0.2f;

		[Range(0,100)]
		public int DropChance;

		public bool DropsOnlyOnce;
		[ShowIf(nameof(DropsOnlyOnce))]
		public string DropOnceId;
	}
}
