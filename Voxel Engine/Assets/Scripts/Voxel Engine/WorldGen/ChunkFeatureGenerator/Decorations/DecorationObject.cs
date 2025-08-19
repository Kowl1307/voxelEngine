using System;
using UnityEngine;

namespace Voxel_Engine.WorldGen.ChunkFeatureGenerator.Decorations
{
    public class DecorationObject : MonoBehaviour
    {
        private Action<DecorationObject> _disposeAction = DefaultDecorationDispose;

        public void SetDisposeOperation(Action<DecorationObject> disposeOperation)
        {
            _disposeAction = disposeOperation ?? DefaultDecorationDispose;
        }

        public void Dispose()
        {
            _disposeAction(this);
        }

        private static void DefaultDecorationDispose(DecorationObject objectToDispose)
        {
            Destroy(objectToDispose.gameObject);
        }
    }
}