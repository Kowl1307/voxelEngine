using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Voxel_Engine.WorldGen.Structures
{
    public abstract class StructureGenerator : MonoBehaviour
    {
        [SerializeField]
        protected Vector3Int structureExtents = Vector3Int.one;

        protected readonly List<Bounds> CachedBounds = new();

        public abstract VoxelType GetStructureVoxelAt(Vector3Int voxelPosition, World world);

        /// <summary>
        /// Checks if the given position is close enough to a Point of Interest (POI) to be considered for generation.
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="world"></param>
        /// <returns></returns>
        public bool IsInPoiRange(Vector3Int worldPosition, World world)
        {
            return (worldPosition - GetClosestPointOfInterest(worldPosition, world)).Abs().ComponentWise(structureExtents, (a,b)=>a<b);
        }

        /// <summary>
        /// Gets the closest point of interest to a world position (not necessarily the object a voxel may originate from!)
        /// </summary>
        /// <param name="voxelPosition"></param>
        /// <param name="world"></param>
        /// <returns></returns>
        public abstract Vector3Int GetClosestPointOfInterest(Vector3Int voxelPosition, World world);

        /// <summary>
        /// Gets all POIs that have the given worldPosition in their bounds.
        /// </summary>
        /// <param name="voxelPosition"></param>
        /// <param name="world"></param>
        /// <returns></returns>
        protected abstract List<Vector3Int> GetClosePointsOfInterest(Vector3Int voxelPosition, World world);
    }
}