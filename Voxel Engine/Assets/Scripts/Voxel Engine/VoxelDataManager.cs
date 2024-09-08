using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Voxel_Engine
{
    public class VoxelDataManager : MonoBehaviour
    {
        public static float TextureOffset = 0.001f;
        public static float TileSizeX, TileSizeY;

        public static Dictionary<VoxelType, TextureData> VoxelTextureDataDictionary =
            new Dictionary<VoxelType, TextureData>();

        public VoxelDataSO textureData;

        private void Awake()
        {
            foreach (var item in textureData.textureDataList.Where(item => !VoxelTextureDataDictionary.ContainsKey(item.VoxelType)))
            {
                VoxelTextureDataDictionary.Add(item.VoxelType, item);
            }

            TileSizeX = textureData.textureSizeX;
            TileSizeY = textureData.textureSizeY;
        }
    }
}
