using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using game.gameplay_core.location.location_save_system;
using UnityEngine;

namespace game.gameplay_core
{
	public class CoreGameDomain
	{
		private LocationDomain _locationDomain;

		private async UniTask Initialize(LocationSaveData saveData)

		{
			await PreloadCoreGameAssets();

			_locationDomain = new LocationDomain();
			_locationDomain.Initialize(saveData);
		}

		public async UniTask PlayOnDebugLocation(string locationId, bool resetState)
		{
			var saveData = new LocationSaveData();
			var savePath = GetSavePath(locationId);
			var playerSavePath = GetPlayerSavePath();

			if (!resetState)
			{
				if (File.Exists(savePath))
				{
					saveData = JsonUtility.FromJson<LocationSaveData>(File.ReadAllText(savePath));
				}

				if (File.Exists(playerSavePath))
				{
					GameStaticContext.Instance.PlayerSave = JsonUtility.FromJson<PlayerSaveData>(File.ReadAllText(playerSavePath));
				}
			}

			await Initialize(saveData);
		}

		public async UniTask PlayOnLocation(string locationId)
		{
			//TODO: load scene
			var locationSavePath = GetSavePath(locationId);
			var saveData = JsonUtility.FromJson<LocationSaveData>(File.ReadAllText(locationSavePath));
			//TODO: load player data as well
			await Initialize(saveData);
		}

		public void SaveCurrentLocation()
		{
			if (_locationDomain == null) return;
			var saveData = _locationDomain.SaveCurrentStateToData();
			var savePath = GetSavePath(GameStaticContext.Instance.PlayerSave.CurrentLocationId);
			var playerSavePath = GetPlayerSavePath();

			var directory = Path.GetDirectoryName(savePath);
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			File.WriteAllText(savePath, JsonUtility.ToJson(saveData, true));
			File.WriteAllText(playerSavePath, JsonUtility.ToJson(GameStaticContext.Instance.PlayerSave, true));
		}

		private string GetSavePath(string locationId)
		{
			return Path.Combine(Application.persistentDataPath, "saves", GameStaticContext.Instance.SaveSlotId, $"{locationId}.json");
		}

		private string GetPlayerSavePath()
		{
			return Path.Combine(Application.persistentDataPath, "saves", GameStaticContext.Instance.SaveSlotId, "player.json");
		}

		public void RespawnAndReloadLocation()
		{
			//TODO: reload scene for maximum consistency
			_locationDomain.RespawnAndReloadLocation();
			SaveCurrentLocation();
		}

		private async UniTask PreloadCoreGameAssets()
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"Preload Start - Frame: {Time.frameCount}");

			await UniTask.WhenAll(
				AddressableManager.PreloadAssetsListAsync(AddressableAssetNames.ItemConfigs, AssetOwner.CoreGame),
				AddressableManager.PreloadAssetsListAsync(AddressableAssetNames.WeaponPrefabNames, AssetOwner.CoreGame),
				AddressableManager.PreloadAssetsListAsync(AddressableAssetNames.ProjectilePrefabs, AssetOwner.CoreGame),
				AddressableManager.PreloadAssetAsync(AddressableAssetNames.Player, AssetOwner.CoreGame),
				AddressableManager.PreloadAssetAsync(AddressableAssetNames.CharacterUi, AssetOwner.CoreGame),
				AddressableManager.PreloadAssetAsync(AddressableAssetNames.FloatingTextView, AssetOwner.CoreGame)
			);

			stringBuilder.AppendLine($"Preload End - Frame: {Time.frameCount}");
			Debug.Log(stringBuilder.ToString());
		}
	}
}
