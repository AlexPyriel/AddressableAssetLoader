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

Add this dependency to `Packages/manifest.json`:

```json
"com.gdf.addressable-asset-loader": "https://github.com/AlexPyriel/AddressableAssetLoader.git#v1.0.1"
```

For development tracking (not recommended for production), use `#main` instead of a tag.

You can also install it from Unity Package Manager:

- `Window -> Package Manager -> + -> Add package from git URL...`
- `https://github.com/AlexPyriel/AddressableAssetLoader.git#v1.0.1`

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

## Assembly Definition Notes

`GDF.AddressableAssetLoader` is a dedicated runtime assembly. Concrete loader subclasses live in
the consuming project; the package exposes only the abstract bases so the DI graph stays strict
about which asset types are available.

## Tests

EditMode tests cover concurrent request deduplication, cached handle reuse, explicit unload, prefab component resolution, and invalid prefab cleanup.

## License

MIT. See `LICENSE.md`.
