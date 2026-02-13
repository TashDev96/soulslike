using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
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
	private static readonly Dictionary<string, AsyncOperationHandle> _handles = new();
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
		for(int i = 0; i < addresses.Length; i++)
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
		if(_handles.TryGetValue(address, out var existingHandle))
		{
			await existingHandle.ToUniTask();
			return (T)existingHandle.Result;
		}

		try
		{
			var handle = Addressables.LoadAssetAsync<T>(address);
			_handles[address] = handle;
			_assetOwners[owner].Add(address);

			await handle.ToUniTask();
			return (T)handle.Result;
		}
		catch(Exception e)
		{
			Debug.LogError($"Failed to load asset at address {address}: {e.Message}");
			_handles.Remove(address);
			return default;
		}
	}

	public static T LoadAssetImmediately<T>(string address, AssetOwner owner)
	{
		if(_handles.TryGetValue(address, out var existingHandle))
		{
			return (T)existingHandle.Result;
		}

		try
		{
			var handle = Addressables.LoadAssetAsync<T>(address);
			var asset = handle.WaitForCompletion();
			_handles[address] = handle;
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
		if(_handles.TryGetValue(address, out var handle))
		{
			if(handle.Status == AsyncOperationStatus.Succeeded)
			{
				return (T)handle.Result;
			}
			throw new Exception($"Error! asset {address} is in status {handle.Status}");
		}
		throw new Exception($"Error! asset {address} was not preloaded");
	}

	public static void ClearMemory(AssetOwner owner)
	{
		foreach(var address in _assetOwners[owner])
		{
			if(_handles.TryGetValue(address, out var handle))
			{
				Addressables.Release(handle);
				_handles.Remove(address);
			}
		}
		_assetOwners[owner].Clear();
	}

	public static void ClearAllMemory()
	{
		foreach(var handle in _handles.Values)
		{
			Addressables.Release(handle);
		}

		_handles.Clear();
		foreach(var list in _assetOwners.Values)
		{
			list.Clear();
		}
	}
}
