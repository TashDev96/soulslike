using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

public enum AssetOwner
{
	Application,
	Game,
	CoreGame,
	Location
}

public static class AddressableManager
{
	private static readonly Dictionary<string, object> _loadedAssets = new();
	private static readonly Dictionary<AssetOwner, List<string>> _assetOwners = new()
	{
		{ AssetOwner.Application, new List<string>() },
		{ AssetOwner.Game, new List<string>() },
		{ AssetOwner.CoreGame, new List<string>() },
		{ AssetOwner.Location, new List<string>() }
	};

	public static async UniTask PreloadAssetsListAsync(string[] addresses, AssetOwner owner)
	{
		var tasks = new UniTask[addresses.Length];
		for (int i = 0; i < addresses.Length; i++)
		{
			tasks[i] = LoadAssetAsync<Object>(addresses[i], owner);
		}
		await UniTask.WhenAll(tasks);
	}

	public static async UniTask PreloadAssetAsync(string address, AssetOwner owner)
	{
		await LoadAssetAsync<Object>(address, owner);
	}

	public static async UniTask<T> LoadAssetAsync<T>(string address, AssetOwner owner)
	{
		if(_loadedAssets.TryGetValue(address, out var loadedAsset))
		{
			return (T)loadedAsset;
		}

		try
		{
			var asset = await Addressables.LoadAssetAsync<T>(address);
			_loadedAssets[address] = asset;
			_assetOwners[owner].Add(address);
			return asset;
		}
		catch(Exception e)
		{
			Debug.LogError($"Failed to load asset at address {address}: {e.Message}");
			return default;
		}
	}

	public static T LoadAssetImmediately<T>(string address, AssetOwner owner)
	{
		if(_loadedAssets.TryGetValue(address, out var loadedAsset))
		{
			return (T)loadedAsset;
		}

		try
		{
			var asset = Addressables.LoadAssetAsync<T>(address).WaitForCompletion();
			_loadedAssets[address] = asset;
			_assetOwners[owner].Add(address);
			return asset;
		}
		catch(Exception e)
		{
			Debug.LogError($"Failed to load asset instantly at address {address}: {e.Message}");
			return default;
		}
	}

	public static T GetPreloadedAsset<T>(string address)
	{
		if(_loadedAssets.TryGetValue(address, out var loadedAsset))
		{
			return (T)loadedAsset;
		}
		throw new Exception($"Error! asset {address} was not preloaded");
	}

	public static void ClearMemory(AssetOwner owner)
	{
		foreach(var address in _assetOwners[owner])
		{
			if(_loadedAssets.TryGetValue(address, out var asset))
			{
				Addressables.Release(asset);
				_loadedAssets.Remove(address);
			}
		}
		_assetOwners[owner].Clear();
	}

	public static void ClearAllMemory()
	{
		foreach(var asset in _loadedAssets.Values)
		{
			Addressables.Release(asset);
		}

		_loadedAssets.Clear();
		foreach(var list in _assetOwners.Values)
		{
			list.Clear();
		}
	}
}
