# Changelog

## [1.0.0] - 2026-05-27

- Initial public release of the package.
- Added public abstract `AddressableAssetLoaderService<TAsset>` for typed Addressables asset loading.
- Added public abstract `AddressablePrefabsLoaderService<TView>` for loading prefab root components.
- Added in-flight request deduplication, cached handle reuse, explicit unload, and dispose cleanup.
- Added EditMode coverage for concurrent requests, caching, unloading, and prefab component resolution.
