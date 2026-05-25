using Cysharp.Threading.Tasks;

namespace AddressableAssetLoader.Internal
{
    /// <summary>
    /// Stores the shared completion state for a single in-flight asset load.
    /// </summary>
    internal sealed class InFlightLoad<TAsset>
        where TAsset : UnityEngine.Object
    {
        /// <summary>
        /// Gets the completion source shared by all callers awaiting the same in-flight load.
        /// </summary>
        internal UniTaskCompletionSource<TAsset> CompletionSource { get; } = new();

        /// <summary>
        /// Gets a value indicating whether the in-flight load was canceled by an unload or dispose action.
        /// </summary>
        internal bool IsCanceled { get; private set; }

        /// <summary>
        /// Marks the in-flight load as canceled.
        /// </summary>
        internal void Cancel()
        {
            IsCanceled = true;
        }
    }
}
