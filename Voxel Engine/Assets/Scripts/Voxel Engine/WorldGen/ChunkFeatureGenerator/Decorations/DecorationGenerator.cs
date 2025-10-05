using System;
using System.Threading.Tasks;
using Kowl.Utils;
using UnityEngine;

namespace Voxel_Engine.WorldGen.ChunkFeatureGenerator.Decorations
{
    /// <summary>
    /// A Decoration is a GameObject that is added to a chunk. This allows disobendience against the voxel grid
    /// </summary>
    public abstract class DecorationGenerator : ChunkFeatureGenerator
    {
        protected static async Task<DecorationObject> InstantiateDecorationOnMainThread(GameObject objectToCreate, Vector3 position, Quaternion rotation, bool isStatic = true, Action<DecorationObject> disposeOperation = null)
        {
            var go = await UnityMainThreadDispatcher.Instance().EnqueueAsync(() => Instantiate(objectToCreate, position, rotation));
            go.isStatic = isStatic;

            return await SetupDecorationObject(go, disposeOperation);
        }

        protected static async Task<DecorationObject> SetupDecorationObject(GameObject objectToSetup, Action<DecorationObject> disposeOperation = null)
        {
            var decorationObject = await UnityMainThreadDispatcher.Instance().EnqueueAsync(objectToSetup.AddComponent<DecorationObject>);
            
            if(disposeOperation != null)
                decorationObject.SetDisposeOperation(disposeOperation);
            
            return decorationObject;
        }
    }
}