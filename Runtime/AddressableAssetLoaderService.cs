using System;
using System.Collections.Concurrent;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AddressableAssetLoader
{
    /// <summary>
    /// Shared base service for loading Addressables assets of a specific Unity object type.
    /// </summary>
    public abstract class AddressableAssetLoaderService<TAsset> : IDisposable
        where TAsset : UnityEngine.Object
    {
        private readonly ConcurrentDictionary<string, AsyncOperationHandle<TAsset>> _loadedAssets = new();
        private readonly ConcurrentDictionary<string, Internal.InFlightLoad<TAsset>> _loadTasks = new();

        /// <summary>
        /// Loads the asset referenced by the provided addressable reference.
        /// </summary>
        /// <param name="reference">Addressable reference that points to the requested asset.</param>
        /// <returns>The loaded asset instance, or <see langword="null"/> when loading cannot start or fails.</returns>
        public async UniTask<TAsset> LoadAssetAsync(AssetReference reference)
        {
            if (!TryGetReferenceKey(reference, out string key))
            {
                Debug.LogError($"[{GetType().Name}] Invalid {nameof(AssetReference)} supplied for {typeof(TAsset).Name}.");
                return null;
            }

            if (_loadedAssets.TryGetValue(key, out AsyncOperationHandle<TAsset> cachedHandle))
            {
                if (cachedHandle.IsValid() && cachedHandle.Status == AsyncOperationStatus.Succeeded)
                    return cachedHandle.Result;

                _loadedAssets.TryRemove(key, out _);
                ReleaseHandle(cachedHandle);
            }

            Internal.InFlightLoad<TAsset> candidate = new();
            Internal.InFlightLoad<TAsset> inFlightLoad = _loadTasks.GetOrAdd(key, candidate);

            if (ReferenceEquals(inFlightLoad, candidate))
                LoadAssetInternalAsync(reference, key, inFlightLoad).Forget();

            return await inFlightLoad.CompletionSource.Task;
        }

        /// <summary>
        /// Releases the loaded asset for the provided reference if it is currently cached.
        /// </summary>
        /// <param name="reference">Addressable reference used as the cache key.</param>
        public void Unload(AssetReference reference)
        {
            if (!TryGetReferenceKey(reference, out string key))
                return;

            if (_loadedAssets.TryRemove(key, out AsyncOperationHandle<TAsset> handle))
                ReleaseHandle(handle);

            if (_loadTasks.TryRemove(key, out Internal.InFlightLoad<TAsset> inFlightLoad))
                inFlightLoad.Cancel();
        }

        /// <summary>
        /// Releases all cached handles and cancels any in-flight loads.
        /// </summary>
        public void Dispose()
        {
            foreach (AsyncOperationHandle<TAsset> handle in _loadedAssets.Values)
                ReleaseHandle(handle);

            foreach (Internal.InFlightLoad<TAsset> inFlightLoad in _loadTasks.Values)
                inFlightLoad.Cancel();

            _loadedAssets.Clear();
            _loadTasks.Clear();
        }

        /// <summary>
        /// Performs the low-level Addressables load operation for the requested reference.
        /// </summary>
        protected virtual async UniTask<AsyncOperationHandle<TAsset>> LoadAssetHandleAsync(AssetReference reference)
        {
            AsyncOperationHandle<TAsset> handle = reference.LoadAssetAsync<TAsset>();
            await handle.ToUniTask();
            return handle;
        }

        /// <summary>
        /// Releases a previously loaded Addressables handle.
        /// </summary>
        protected virtual void ReleaseHandle(AsyncOperationHandle<TAsset> handle)
        {
            if (handle.IsValid())
                Addressables.Release(handle);
        }

        private async UniTaskVoid LoadAssetInternalAsync(
            AssetReference reference,
            string key,
            Internal.InFlightLoad<TAsset> inFlightLoad)
        {
            try
            {
                AsyncOperationHandle<TAsset> handle = await LoadAssetHandleAsync(reference);

                if (!handle.IsValid() || handle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"[{GetType().Name}] Failed to load {typeof(TAsset).Name} for reference '{reference.AssetGUID}'.");
                    ReleaseHandle(handle);
                    inFlightLoad.CompletionSource.TrySetResult(null);
                    return;
                }

                if (inFlightLoad.IsCanceled)
                {
                    ReleaseHandle(handle);
                    inFlightLoad.CompletionSource.TrySetResult(null);
                    return;
                }

                _loadedAssets[key] = handle;
                inFlightLoad.CompletionSource.TrySetResult(handle.Result);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[{GetType().Name}] Exception while loading {typeof(TAsset).Name} for reference '{reference?.AssetGUID}':\n{exception}");
                inFlightLoad.CompletionSource.TrySetException(exception);
            }
            finally
            {
                if (_loadTasks.TryGetValue(key, out Internal.InFlightLoad<TAsset> currentLoad) &&
                    ReferenceEquals(currentLoad, inFlightLoad))
                    _loadTasks.TryRemove(key, out _);
            }
        }

        private static bool TryGetReferenceKey(AssetReference reference, out string key)
        {
            key = reference?.AssetGUID;
            return !string.IsNullOrWhiteSpace(key);
        }
    }
}
