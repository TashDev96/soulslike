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

		private async UniTask PreloadCoreGameAssets()
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"Frame Sequence: {Time.frameCount}");
			await AddressableManager.PreloadAssetsListAsync(AddressableAssetNames.ItemConfigs, AssetOwner.CoreGame);
			stringBuilder.AppendLine($"Frame Sequence: {Time.frameCount}");
			await AddressableManager.PreloadAssetsListAsync(AddressableAssetNames.WeaponPrefabNames, AssetOwner.CoreGame);
			stringBuilder.AppendLine($"Frame Sequence: {Time.frameCount}");
			await AddressableManager.PreloadAssetsListAsync(AddressableAssetNames.ProjectilePrefabs, AssetOwner.CoreGame);
			stringBuilder.AppendLine($"Frame Sequence: {Time.frameCount}");
			await AddressableManager.PreloadAssetAsync(AddressableAssetNames.Player, AssetOwner.CoreGame);
			stringBuilder.AppendLine($"Frame Sequence: {Time.frameCount}");
			await AddressableManager.PreloadAssetAsync(AddressableAssetNames.CharacterUi, AssetOwner.CoreGame);
			stringBuilder.AppendLine($"Frame Sequence: {Time.frameCount}");
			await AddressableManager.PreloadAssetAsync(AddressableAssetNames.FloatingTextView, AssetOwner.CoreGame);
			Debug.LogError(stringBuilder.ToString());
		}

		public void RespawnAndReloadLocation()
		{
			//TODO: reload scene for maximum consistency
			_locationDomain.RespawnAndReloadLocation();
		}
	}
}
