# Changelog

## [1.0.2] - 2026-11-02

### Fixed

- Added missing `LICENSE.md.meta` so Unity no longer logs an "immutable folder has no meta" warning when the package is consumed as a UPM git dependency.

## [1.0.1] - 2026-11-01

### Changed

- Added `LICENSE.md` (MIT) to the package root.
- README now includes an `Assembly Definition Notes` section and a `License` footer section matching the sibling GDF packages.
- Removed stale empty `Interfaces/` and `Internal/` directories from the package root; the actual code lives under `Runtime/`.

## [1.0.0] - 2026-05-27

- Initial public release of the package.
- Added public abstract `AddressableAssetLoaderService<TAsset>` for typed Addressables asset loading.
- Added public abstract `AddressablePrefabsLoaderService<TView>` for loading prefab root components.
- Added in-flight request deduplication, cached handle reuse, explicit unload, and dispose cleanup.
- Added EditMode coverage for concurrent requests, caching, unloading, and prefab component resolution.
