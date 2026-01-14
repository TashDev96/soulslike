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

		public async UniTask PlayOnDebugLocation()
		{
			var saveData = new LocationSaveData();
			await Initialize(saveData);
		}

		public async UniTask PlayOnLocation(string locationId)
		{
			//TODO: load scene
			var locationSavePath = Path.Combine(GameStaticContext.Instance.SaveSlotId, locationId);
			var saveData = JsonUtility.FromJson<LocationSaveData>(File.ReadAllText(locationSavePath));
			await Initialize(saveData);
		}

		public void RespawnAndReloadLocation()
		{
			//TODO: reload scene for maximum consistency
			_locationDomain.RespawnAndReloadLocation();
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
