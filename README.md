# GDF Addressable Asset Loader

Reusable Addressables-backed base services for typed asset and prefab loading in Unity.

Package name: `com.gdf.addressable-asset-loader`  
Assembly name: `GDF.AddressableAssetLoader`

## Features

- Public abstract `AddressableAssetLoaderService<TAsset>` base service for any `UnityEngine.Object` asset type.
- Public abstract `AddressablePrefabsLoaderService<TView>` base service for prefab-root component loading.
- Deduplicates parallel requests for the same addressable reference.
- Caches successful load handles for repeated lookups.
- Supports explicit `Unload(...)` by the original `AssetReference`.
- Releases all cached handles on `Dispose()`.

## Requirements

- Unity `6000.0+`
- `com.unity.addressables` `2.8.1+`
- `com.cysharp.unitask`

## Installation (UPM via Git)

```json
"com.gdf.addressable-asset-loader": "https://github.com/AlexPyriel/AddressableAssetLoader.git#v1.0.0"
```

## Usage

Create thin concrete loaders for the asset types your project needs.

```csharp
using AddressableAssetLoader;
using UnityEngine;

public sealed class AddressableAudioClipLoaderService : AddressableAssetLoaderService<AudioClip>
{
}

public sealed class AddressableGunLoaderService : AddressablePrefabsLoaderService<GunView>
{
}
```

These concrete services can be registered directly in your DI container.

```csharp
builder.Register<AddressableAudioClipLoaderService>(Lifetime.Singleton);
builder.Register<AddressableGunLoaderService>(Lifetime.Singleton);
```

`AddressableAssetLoaderService<TAsset>` is intended for raw addressable assets such as `AudioClip`, `Sprite`, `Material`, `ScriptableObject`, or `GameObject`.

`AddressablePrefabsLoaderService<TView>` is intended for addressable prefabs where callers want a required root component instead of the raw `GameObject`.

## Tests

EditMode tests cover concurrent request deduplication, cached handle reuse, explicit unload, prefab component resolution, and invalid prefab cleanup.
