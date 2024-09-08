using System;
using System.Collections.Generic;
using UnityEngine;

namespace Voxel_Engine
{
    [CreateAssetMenu(fileName = "Voxel Data", menuName = "Data/Voxel Data")]
    public class VoxelDataSO : ScriptableObject
    {
        public float textureSizeX, textureSizeY;
        public List<TextureData> textureDataList;
    }

    [Serializable]
    public class TextureData
    {
        public VoxelType VoxelType;
        //If different sides, add 3 more sides
        public Vector2Int up, down, side;
        public bool IsSolid = true;
        public bool GeneratesCollider = true;
    }
}