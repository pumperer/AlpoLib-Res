using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
#if USE_ASSETBUNDLE
using AssetBundles;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;

namespace alpoLib.Res
{
	public enum PrefabPathSource
	{
		Resources,
		Addressable,
#if USE_ASSETBUNDLE
		AssetBundle,
#endif
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class PrefabPathAttribute : Attribute
	{
		public string Path { get; }
		public PrefabPathSource Source { get; }
		public string SubPath { get; }

		public PrefabPathAttribute(string path, PrefabPathSource source = PrefabPathSource.Addressable)
		{
			Path = path;
			Source = source;

#if USE_ASSETBUNDLE
			if (source != PrefabPathSource.AssetBundle)
				return;

			var lastIndexOfSeparator = path.LastIndexOf('/');
			if (lastIndexOfSeparator <= -1)
				return;
			Path = path[(lastIndexOfSeparator + 1)..].Replace(".prefab", string.Empty);
			SubPath = $"ab/{path[..lastIndexOfSeparator].ToLowerInvariant()}";
#endif
		}
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class MultiPrefabPathAttribute : Attribute
	{
		public string Key { get; }
		public string Path { get; }
		public PrefabPathSource Source { get; }
		public string SubPath { get; }

		public MultiPrefabPathAttribute(string key, string path, PrefabPathSource source = PrefabPathSource.Addressable)
		{
			Key = key;
			Path = path;
			Source = source;

#if USE_ASSETBUNDLE
			if (source != PrefabPathSource.AssetBundle)
				return;

			var lastIndexOfSeparator = path.LastIndexOf('/');
			if (lastIndexOfSeparator <= -1)
				return;
			Path = path[(lastIndexOfSeparator + 1)..].Replace(".prefab", string.Empty);
			SubPath = $"ab/{path[..lastIndexOfSeparator].ToLowerInvariant()}";
#endif
		}
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class RandomPrefabPathAttribute : Attribute
	{
		public string Path { get; }
		public PrefabPathSource Source { get; }
		public string SubPath { get; }

		public RandomPrefabPathAttribute(string path, PrefabPathSource source = PrefabPathSource.Addressable)
		{
			Path = path;
			Source = source;

#if USE_ASSETBUNDLE
			if (source != PrefabPathSource.AssetBundle)
				return;

			var lastIndexOfSeparator = path.LastIndexOf('/');
			if (lastIndexOfSeparator <= -1)
				return;
			Path = path[(lastIndexOfSeparator + 1)..].Replace(".prefab", string.Empty);
			SubPath = $"ab/{path[..lastIndexOfSeparator].ToLowerInvariant()}";
#endif
		}
	}

	public static class GenericPrefab
	{
		// private static string GetPrefabPathFromKey(Type type, string key)
		// {
		// 	var attrs = type.GetCustomAttributes<MultiPrefabPathAttribute>().ToDictionary(a => a.Key, a => a.Path);
		// 	return attrs.TryGetValue(key, out var path) ? path : string.Empty;
		// }

		public static T LoadPrefab<T>(string key = null) where T : UnityEngine.Object
		{
			var type = typeof(T);
			return (T)LoadPrefab(type, key);
		}

		public static UnityEngine.Object LoadPrefab(Type type, string key = null)
		{
			if (string.IsNullOrEmpty(key))
			{
				// 기본 패스 검사
				if (type.GetCustomAttribute<PrefabPathAttribute>() is { } attr)
				{
					if (attr.Source == PrefabPathSource.Addressable)
						return LoadPrefab_Addressable(type, attr.Path);
					if (attr.Source == PrefabPathSource.Resources)
						return Resources.Load(attr.Path.Replace(".prefab", string.Empty));
					return null;
				}
			}
			else
			{
				// 멀티 패스 검사
				var multiPathDic = type.GetCustomAttributes<MultiPrefabPathAttribute>()
					.ToDictionary(a => a.Key, a => a);
				if (multiPathDic.TryGetValue(key, out var attr))
				{
					if (attr.Source == PrefabPathSource.Addressable)
						return LoadPrefab_Addressable(type, attr.Path);
					if (attr.Source == PrefabPathSource.Resources)
						return Resources.Load(attr.Path.Replace(".prefab", string.Empty));
					return null;
				}
			}

			// 랜덤 패스 검사
			var attrs = type.GetCustomAttributes<RandomPrefabPathAttribute>().ToList();
			var count = attrs.Count;
			if (count <= 0)
				return null;
			var index = Random.Range(0, count);
			var randomAttr = attrs[index];
			if (randomAttr.Source == PrefabPathSource.Addressable)
				return LoadPrefab_Addressable(type, randomAttr.Path);
			if (randomAttr.Source == PrefabPathSource.Resources)
				return Resources.Load(randomAttr.Path.Replace(".prefab", string.Empty));
			return null;
		}

		public static T InstantiatePrefab<T>(string key = null, Transform parent = null) where T : UnityEngine.Object
		{
			var type = typeof(T);
			return (T)InstantiatePrefab(type, key, parent);
		}

		public static UnityEngine.Object InstantiatePrefab(Type type, string key = null, Transform parent = null)
		{
			if (string.IsNullOrEmpty(key))
			{
				// 기본 패스 검사
				if (type.GetCustomAttribute<PrefabPathAttribute>() is { } attr)
				{
					if (attr.Source == PrefabPathSource.Addressable)
						return LoadAndInstantiate_Addressable(type, attr.Path, parent);
					if (attr.Source == PrefabPathSource.Resources)
						return LoadAndInstantiate_Resources(type, attr.Path.Replace(".prefab", string.Empty), parent);
#if USE_ASSETBUNDLE
					if (attr.Source == PrefabPathSource.AssetBundle)
						throw new NotSupportedException();
#endif
					return null;
				}
			}
			else
			{
				// 멀티 패스 검사
				var multiPathDic = type.GetCustomAttributes<MultiPrefabPathAttribute>()
					.ToDictionary(a => a.Key, a => a);
				if (multiPathDic.TryGetValue(key, out var attr))
				{
					if (attr.Source == PrefabPathSource.Addressable)
						return LoadAndInstantiate_Addressable(type, attr.Path, parent);
					if (attr.Source == PrefabPathSource.Resources)
						return LoadAndInstantiate_Resources(type, attr.Path.Replace(".prefab", string.Empty), parent);
#if USE_ASSETBUNDLE
					if (attr.Source == PrefabPathSource.AssetBundle)
						throw new NotSupportedException();
#endif
					return null;
				}
			}

			// 랜덤 패스 검사
			var attrs = type.GetCustomAttributes<RandomPrefabPathAttribute>().ToList();
			var count = attrs.Count;
			if (count <= 0)
				return null;
			var index = Random.Range(0, count);
			var randomAttr = attrs[index];
			if (randomAttr.Source == PrefabPathSource.Addressable)
				return LoadAndInstantiate_Addressable(type, randomAttr.Path, parent);
			if (randomAttr.Source == PrefabPathSource.Resources)
				return LoadAndInstantiate_Resources(type, randomAttr.Path.Replace(".prefab", string.Empty), parent);
#if USE_ASSETBUNDLE
			if (randomAttr.Source == PrefabPathSource.AssetBundle)
				throw new NotSupportedException();
#endif
			return null;
		}

		public static async Task<T> InstantiatePrefabAsync<T>(string key = null, Transform parent = null)
			where T : UnityEngine.Object
		{
			return (T)await LoadAndInstantiateAsync<T>(key, parent);
		}

		public static async Task<UnityEngine.Object> InstantiatePrefabAsync(Type type, string key = null,
			Transform parent = null)
		{
			return await LoadAndInstantiateAsync(type, key, parent);
		}

		// public static T InstantiatePrefab_Resources<T>(Transform parent = null) where T : MonoBehaviour
		// {
		// 	return InstantiatePrefab_Resources(typeof(T), parent) as T;
		// }

		// public static T InstantiatePrefab_Resources<T>(string key, Transform parent = null) where T : MonoBehaviour
		// {
		// 	var path = GetPrefabPathFromKey(typeof(T), key);
		// 	return LoadAndInstantiate_Resources<T>(path, parent);
		// }

		// public static UnityEngine.Object InstantiatePrefab_Resources(Type type, Transform parent = null)
		// {
		// 	string prefabKey;
		// 	if (type.GetCustomAttribute<PrefabPathAttribute>() is { } attr)
		// 	{
		// 		LoadAndInstantiateAsync_Addressable<>()
		// 		prefabKey = attr.Path.Replace(".prefab", string.Empty);
		// 		return LoadAndInstantiate_Resources(type, prefabKey, parent);
		// 	}
		//
		// 	var attrs = type.GetCustomAttributes<RandomPrefabPathAttribute>().ToList();
		// 	var count = attrs.Count;
		// 	if (count <= 0)
		// 		return null;
		//
		// 	var index = Random.Range(0, count);
		// 	var randomAttr = attrs[index];
		// 	prefabKey = randomAttr.Path.Replace(".prefab", string.Empty);
		// 	return LoadAndInstantiate_Resources(type, prefabKey, parent);
		// }

		// public static Task<T> LoadPrefabAsync_Addressable<T>(string key, Transform parent = null)
		// 	where T : MonoBehaviour
		// {
		// 	var path = GetPrefabPathFromKey(typeof(T), key);
		// 	return LoadAndInstantiateAsync_Addressable<T>(path, parent);
		// }

		// public static void LoadPrefabAsync_Addressable<T>(string key, Action<T> completeAction, Transform parent = null)
		// 	where T : MonoBehaviour
		// {
		// 	var path = GetPrefabPathFromKey(typeof(T), key);
		// 	LoadAndInstantiateAsync_Addressable(typeof(T), path, t => completeAction?.Invoke(t as T), parent);
		// }
		//
		// public static T InstantiatePrefab_Addressable<T>(Transform parent = null) where T : MonoBehaviour
		// {
		// 	var type = typeof(T);
		// 	if (type.GetCustomAttribute<PrefabPathAttribute>() is { } attr)
		// 	{
		// 		return LoadAndInstantiate_Addressable<T>(attr.Path, parent);
		// 	}
		//
		// 	var attrs = type.GetCustomAttributes<RandomPrefabPathAttribute>().ToList();
		// 	var count = attrs.Count;
		// 	if (count <= 0)
		// 		return null;
		//
		// 	var index = Random.Range(0, count);
		// 	var randomAttr = attrs[index];
		// 	return LoadAndInstantiate_Addressable<T>(randomAttr.Path, parent);
		// }

		// public static void LoadPrefabAsync_Addressable<T>(Action<T> completeAction, Transform parent = null)
		// 	where T : MonoBehaviour
		// {
		// 	LoadPrefabAsync_Addressable(typeof(T), t => completeAction?.Invoke(t as T), parent);
		// }

		// public static void LoadPrefabAsync_Addressable(Type type, Action<MonoBehaviour> completeAction,
		// 	Transform parent = null)
		// {
		// 	if (type.GetCustomAttribute<PrefabPathAttribute>() is { } attr)
		// 	{
		// 		LoadAndInstantiateAsync_Addressable(type, attr.Path, completeAction, parent);
		// 		return;
		// 	}
		//
		// 	var attrs = type.GetCustomAttributes<RandomPrefabPathAttribute>().ToList();
		// 	var count = attrs.Count;
		// 	if (count <= 0)
		// 	{
		// 		completeAction?.Invoke(null);
		// 		return;
		// 	}
		//
		// 	var index = Random.Range(0, count);
		// 	var randomAttr = attrs[index];
		// 	LoadAndInstantiateAsync_Addressable(type, randomAttr.Path, completeAction, parent);
		// }

		// private static T LoadAndInstantiate_Resources<T>(string path, Transform parent = null) where T : MonoBehaviour
		// {
		// 	return LoadAndInstantiate_Resources(typeof(T), path, parent) as T;
		// }

		// private static T LoadPrefab_Addressable<T>(string path) where T : UnityEngine.Object
		// {
		// 	var type = typeof(T);
		// 	return (T)LoadPrefab_Addressable(type, path);
		// }

		private static UnityEngine.Object LoadPrefab_Addressable(Type type, string path)
		{
			var handle = Addressables.LoadAssetAsync<GameObject>(path);
			var go = handle.WaitForCompletion();
			if (!go)
				return null;
			var comp = go.GetComponent(type) as UnityEngine.Object;
			if (!comp)
				return null;
			return comp;
		}

		private static UnityEngine.Object LoadAndInstantiate_Resources(Type type, string path, Transform parent = null)
		{
			var prefab = Resources.Load(path);
			if (prefab == null)
				return null;
			return InstantiatePrefab(type, prefab, parent);
		}

		// private static T LoadAndInstantiate_Addressable<T>(string path, Transform parent = null) where T : MonoBehaviour
		// {
		// 	var type = typeof(T);
		// 	var handle =
		// 		Addressables.InstantiateAsync(path, new Vector3(-100000, -100000, 0), Quaternion.identity, parent);
		// 	var go = handle.WaitForCompletion();
		// 	if (!go)
		// 		return null;
		// 	var comp = go.GetComponent(type) as MonoBehaviour;
		// 	if (!comp)
		// 		return null;
		//
		// 	go.name = comp.name;
		//
		// 	return (T)comp;
		// }

		private static async Task<UnityEngine.Object> LoadAndInstantiateAsync<T>(string key = null,
			Transform parent = null)
			where T : UnityEngine.Object
		{
			var type = typeof(T);
			return await LoadAndInstantiateAsync(type, key, parent);
		}

		private static async Task<UnityEngine.Object> LoadAndInstantiateAsync(Type type, string key = null,
			Transform parent = null)
		{
			var tcs = new TaskCompletionSource<UnityEngine.Object>();
			var source = PrefabPathSource.Addressable;
			var path = string.Empty;
			var subPath = string.Empty;
			var triggered = false;

			if (string.IsNullOrEmpty(key))
			{
				// 기본 패스 검사
				if (type.GetCustomAttribute<PrefabPathAttribute>() is { } attr)
				{
					source = attr.Source;
					path = attr.Path;
					subPath = attr.SubPath;
					triggered = true;
				}
			}
			else
			{
				// 멀티 패스 검사
				var multiPathDic = type.GetCustomAttributes<MultiPrefabPathAttribute>()
					.ToDictionary(a => a.Key, a => a);
				if (multiPathDic.TryGetValue(key, out var attr))
				{
					source = attr.Source;
					path = attr.Path;
					subPath = attr.SubPath;
					triggered = true;
				}
			}

			// 랜덤 패스 검사
			if (!triggered)
			{
				var attrs = type.GetCustomAttributes<RandomPrefabPathAttribute>().ToList();
				var count = attrs.Count;
				if (count > 0)
				{
					var index = Random.Range(0, count);
					var randomAttr = attrs[index];
					source = randomAttr.Source;
					path = randomAttr.Path;
					subPath = randomAttr.SubPath;
					triggered = true;
				}
			}

			if (triggered)
				Func(type, source, path, subPath, tcs.SetResult, parent);
			else
				tcs.SetResult(null);

			return await tcs.Task;

			void Func(Type t, PrefabPathSource s, string p, string sp, Action<UnityEngine.Object> cb,
				Transform parentInner)
			{
				switch (s)
				{
					case PrefabPathSource.Addressable:
					{
						LoadAndInstantiate_Addressable_Async(t, p, cb, parentInner);
						break;
					}
					case PrefabPathSource.Resources:
					{
						var obj = LoadAndInstantiate_Resources(t, p.Replace(".prefab", string.Empty), parent);
						cb?.Invoke(obj);
						break;
					}
#if USE_ASSETBUNDLE
					case PrefabPathSource.AssetBundle:
						CoroutineTaskManager.AddTask(
							LoadAndInstantiate_AssetBundle_Async(t, p.Replace(".prefab", string.Empty), sp, cb, parent));
						break;
#endif
					default:
						cb?.Invoke(null);
						break;
				}
			}
		}

		private static void LoadAndInstantiate_Addressable_Async(Type type, string path,
			Action<UnityEngine.Object> callback,
			Transform parent = null)
		{
			var handle = Addressables.InstantiateAsync(path, new Vector3(0, 0, 0), Quaternion.identity, parent);
			handle.Completed += operationHandle =>
			{
				var go = operationHandle.Result;
				if (!go)
				{
					callback?.Invoke(null);
					return;
				}

				var comp = go.GetComponent(type);
				if (!comp)
				{
					callback?.Invoke(null);
					return;
				}

				go.name = comp.name;
				callback?.Invoke(comp);
			};
		}

#if USE_ASSETBUNDLE
		private static IEnumerator LoadAndInstantiate_AssetBundle_Async(Type type, string path,
			string subPath, Action<UnityEngine.Object> callback, Transform parent = null)
		{
			var operation = AssetBundleManager.LoadAssetAsync(subPath, path, typeof(UnityEngine.Object));
			yield return operation;
			var obj = operation.GetAsset();
			if (!obj)
			{
				callback?.Invoke(null);
				yield break;
			}

			var instance = InstantiatePrefab(type, obj, parent);
			callback?.Invoke(instance);
		}
#endif

		private static UnityEngine.Object LoadAndInstantiate_Addressable(Type type, string path,
			Transform parent = null)
		{
			var handle = Addressables.InstantiateAsync(path, Vector3.zero, Quaternion.identity, parent);
			var go = handle.WaitForCompletion();
			if (!go)
				return null;
			var comp = go.GetComponent(type);
			if (!comp)
				return null;
			go.name = comp.name;
			return comp;
		}

		private static UnityEngine.Object InstantiatePrefab(Type type, UnityEngine.Object prefab,
			Transform parent = null)
		{
			if (!prefab)
				return null;

			GameObject go;
			if (!parent)
			{
				go = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
			}
			else
			{
				go = UnityEngine.Object.Instantiate(prefab, parent.transform.position, Quaternion.identity, parent) as
					GameObject;
			}

			if (!go)
				return null;

			var comp = go.GetComponent(type);
			go.name = prefab.name;

			return comp;
		}

		//private static T InstantiatePrefab<T>(UnityEngine.Object prefab, Transform parent = null) where T : MonoBehaviour
		//{
		//	return InstantiatePrefab(typeof(T), prefab, parent) as T;
		//}
	}
}