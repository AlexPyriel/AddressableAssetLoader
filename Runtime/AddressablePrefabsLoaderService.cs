using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace AddressableAssetLoader
{
    /// <summary>
    /// Shared base service for loading addressable prefabs and resolving a required root component.
    /// </summary>
    /// <typeparam name="TView">Component type that must exist on the loaded prefab root.</typeparam>
    public abstract class AddressablePrefabsLoaderService<TView> : AddressableAssetLoaderService<GameObject>
        where TView : Component
    {
        /// <summary>
        /// Loads a prefab and returns its required root component.
        /// </summary>
        /// <param name="reference">Addressable prefab reference to load.</param>
        /// <returns>The required component, or <see langword="null"/> when loading or resolution fails.</returns>
        public async UniTask<TView> LoadPrefabAsync(AssetReferenceGameObject reference)
        {
            GameObject prefab = await LoadAssetAsync(reference);
            if (prefab == null)
                return null;

            if (prefab.TryGetComponent(out TView view))
                return view;

            Debug.LogError($"[{GetType().Name}] Loaded prefab for reference '{reference.AssetGUID}' does not contain required component '{typeof(TView).Name}'.");
            Unload(reference);
            return null;
        }
    }
}
