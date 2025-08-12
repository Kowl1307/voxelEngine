using UnityEngine;

namespace Voxel_Engine.WorldGen.ChunkFeatureGenerator.Decorations
{
    /// <summary>
    /// A Decoration is a GameObject that is added to a chunk. This allows disobendience against the voxel grid
    /// </summary>
    public abstract class DecorationGenerator : MonoBehaviour, IChunkFeatureGenerator
    {
        public abstract void Handle(ChunkData chunkData);

        protected static GameObject InstantiateGameObjectOnMainThread(GameObject goToCreate, Vector3 position, Quaternion rotation)
        {
            return UnityMainThreadDispatcher.Instance().EnqueueAsync(() => Instantiate(goToCreate, position, rotation)).Result;
        }
    }
}