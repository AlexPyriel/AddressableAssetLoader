using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.TestTools;

namespace AddressableAssetLoader.Tests.EditMode
{
    public sealed class AddressableAssetLoaderServiceTests
    {
        private const string AssetGuid = "00000000000000000000000000000001";

        [Test]
        public async Task LoadAssetAsync_WithConcurrentRequests_StartsSingleLoad()
        {
            DeferredGameObjectLoader service = new();
            AssetReferenceGameObject reference = new(AssetGuid);
            GameObject prefab = new("Prefab");

            try
            {
                UniTask<GameObject> firstLoad = service.LoadAssetAsync(reference);
                UniTask<GameObject> secondLoad = service.LoadAssetAsync(reference);

                Assert.That(service.LoadCount, Is.EqualTo(1));

                service.Complete(prefab);

                Assert.That(await firstLoad, Is.SameAs(prefab));
                Assert.That(await secondLoad, Is.SameAs(prefab));
            }
            finally
            {
                service.Dispose();
                Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public async Task LoadAssetAsync_WhenLoadedTwice_ReusesCachedHandle()
        {
            ImmediateGameObjectLoader service = new();
            AssetReferenceGameObject reference = new(AssetGuid);
            GameObject prefab = new("Prefab");
            service.Asset = prefab;

            try
            {
                Assert.That(await service.LoadAssetAsync(reference), Is.SameAs(prefab));
                Assert.That(await service.LoadAssetAsync(reference), Is.SameAs(prefab));
                Assert.That(service.LoadCount, Is.EqualTo(1));
            }
            finally
            {
                service.Dispose();
                Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public async Task Unload_WhenAssetIsLoaded_ReleasesHandle()
        {
            ImmediateGameObjectLoader service = new();
            AssetReferenceGameObject reference = new(AssetGuid);
            GameObject prefab = new("Prefab");
            service.Asset = prefab;

            try
            {
                await service.LoadAssetAsync(reference);

                service.Unload(reference);

                Assert.That(service.ReleaseCount, Is.EqualTo(1));
            }
            finally
            {
                service.Dispose();
                Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public async Task LoadPrefabAsync_WhenRootContainsView_ReturnsView()
        {
            ImmediateViewLoader service = new();
            AssetReferenceGameObject reference = new(AssetGuid);
            GameObject prefab = new("ViewPrefab");
            TestView expected = prefab.AddComponent<TestView>();
            service.Asset = prefab;

            try
            {
                Assert.That(await service.LoadPrefabAsync(reference), Is.SameAs(expected));
            }
            finally
            {
                service.Dispose();
                Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public async Task LoadPrefabAsync_WhenRootDoesNotContainView_ReleasesPrefabAndReturnsNull()
        {
            ImmediateViewLoader service = new();
            AssetReferenceGameObject reference = new(AssetGuid);
            GameObject prefab = new("InvalidPrefab");
            service.Asset = prefab;

            try
            {
                LogAssert.Expect(LogType.Error, new Regex("does not contain required component"));

                TestView result = await service.LoadPrefabAsync(reference);

                Assert.That(result, Is.Null);
                Assert.That(service.ReleaseCount, Is.EqualTo(1));
            }
            finally
            {
                service.Dispose();
                Object.DestroyImmediate(prefab);
            }
        }

        private sealed class DeferredGameObjectLoader : AddressableAssetLoaderService<GameObject>
        {
            private readonly UniTaskCompletionSource<AsyncOperationHandle<GameObject>> _completionSource = new();

            internal int LoadCount { get; private set; }

            internal void Complete(GameObject asset)
            {
                _completionSource.TrySetResult(Addressables.ResourceManager.CreateCompletedOperation(asset, string.Empty));
            }

            protected override async UniTask<AsyncOperationHandle<GameObject>> LoadAssetHandleAsync(AssetReference reference)
            {
                LoadCount++;
                return await _completionSource.Task;
            }
        }

        private class ImmediateGameObjectLoader : AddressableAssetLoaderService<GameObject>
        {
            internal GameObject Asset { private get; set; }

            internal int LoadCount { get; private set; }

            internal int ReleaseCount { get; private set; }

            protected override UniTask<AsyncOperationHandle<GameObject>> LoadAssetHandleAsync(AssetReference reference)
            {
                LoadCount++;
                AsyncOperationHandle<GameObject> handle = Addressables.ResourceManager.CreateCompletedOperation(Asset, string.Empty);
                return UniTask.FromResult(handle);
            }

            protected override void ReleaseHandle(AsyncOperationHandle<GameObject> handle)
            {
                ReleaseCount++;
                base.ReleaseHandle(handle);
            }
        }

        private sealed class ImmediateViewLoader : AddressablePrefabsLoaderService<TestView>
        {
            internal GameObject Asset { private get; set; }

            internal int ReleaseCount { get; private set; }

            protected override UniTask<AsyncOperationHandle<GameObject>> LoadAssetHandleAsync(AssetReference reference)
            {
                AsyncOperationHandle<GameObject> handle = Addressables.ResourceManager.CreateCompletedOperation(Asset, string.Empty);
                return UniTask.FromResult(handle);
            }

            protected override void ReleaseHandle(AsyncOperationHandle<GameObject> handle)
            {
                ReleaseCount++;
                base.ReleaseHandle(handle);
            }
        }

    }

    internal sealed class TestView : MonoBehaviour
    {
    }
}
