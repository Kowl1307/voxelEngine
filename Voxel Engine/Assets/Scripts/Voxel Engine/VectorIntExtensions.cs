using System;
using UnityEngine;

namespace Voxel_Engine
{
    public static class VectorIntExtensions
    {
        public static Vector3Int Abs(this Vector3Int vector)
        {
            return new Vector3Int(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
        }

        public static bool ComponentWise(this Vector3Int vector, Vector3Int other, Func<int, int, bool> comparisonFunction)
        {
            return comparisonFunction(vector.x, other.x) && comparisonFunction(vector.y, other.y) &&
                   comparisonFunction(vector.z, other.z);
        }

        public static Vector2Int XZ(this Vector3Int vector)
        {  
            return new Vector2Int(vector.x, vector.z);
        }


        public static Vector3Int AsX0Z(this Vector2Int vector)
        {
            return new Vector3Int(vector.x, 0, vector.y);
        }
    }
}