﻿using UnityEngine;

namespace Voxel_Engine.WorldGen
{
    public class BiomeCenter
    {
        public Vector3Int Position;
        public float Temperature;

        public BiomeCenter(Vector3Int position)
        {
            Position = position;
        }
    }
}