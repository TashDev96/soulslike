using System;
using System.Globalization;
using dream_lib.src.utils.data_types;
using Sirenix.OdinInspector;

namespace game.gameplay_core.inventory.serialized_data
{
	[Serializable]
	public sealed class InventoryItemSaveData
	{
		[ValueDropdown("@ConfigsResolver.GetAllItemConfigs()")]
		public string ConfigId;
		public bool IsInitialized;
		public SerializableDictionary<string, string> ComponentsData = new();

		public float GetFloat(string key)
		{
			return float.Parse(ComponentsData[key]);
		}

		public int GetInt(string key)
		{
			return int.Parse(ComponentsData[key]);
		}

		public bool GetBool(string key)
		{
			return bool.Parse(ComponentsData[key]);
		}

		public string GetString(string key)
		{
			return ComponentsData[key];
		}

		public void SetFloat(string key, float value)
		{
			ComponentsData[key] = value.ToString(CultureInfo.InvariantCulture);
		}

		public void SetInt(string key, int value)
		{
			ComponentsData[key] = value.ToString(CultureInfo.InvariantCulture);
		}

		public void SetBool(string key, bool value)
		{
			ComponentsData[key] = value.ToString(CultureInfo.InvariantCulture);
		}

		public void SetString(string key, string value)
		{
			ComponentsData[key] = value;
		}
	}
}
