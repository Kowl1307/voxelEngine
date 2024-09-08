using System;
using System.Collections.Generic;
using UnityEngine;

namespace Voxel_Engine.WorldGen.Biomes
{
    public static class BiomeCenterFinder
    {
        private static List<Vector2Int> directions = new List<Vector2Int>
        {
            new(0,1), //N
            new(1,1), //NE
            new(1,0), //E
            new(1,-1), //SE
            new(0,-1), //S
            new(-1,-1), //SW
            new(-1,0), //W
            new(-1,1), //NW
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="playerPos"></param>
        /// <param name="drawRange">Render range of world</param>
        /// <param name="chunkSize">Size of one chunk</param>
        /// <returns></returns>
        public static List<Vector3Int> CalculateBiomeCenters(Vector3 playerPos, int drawRange, int chunkSize)
        {
            var biomeLength = drawRange * chunkSize;
            var origin = new Vector3Int(Mathf.RoundToInt(playerPos.x / biomeLength) * biomeLength, 0, Mathf.RoundToInt(playerPos.z / biomeLength) * biomeLength);
            var biomeCentersTemp = new HashSet<Vector3Int> { origin };

            foreach (var offsetXZ in directions)
            {
                var newBiomePoint1 = new Vector3Int(origin.x + offsetXZ.x * biomeLength, 0,
                    origin.z + offsetXZ.y * biomeLength);
                var newBiomePoint2 = new Vector3Int(origin.x + offsetXZ.x * 2 * biomeLength, 0,
                    origin.z + offsetXZ.y * biomeLength);
                var newBiomePoint3 = new Vector3Int(origin.x + offsetXZ.x * biomeLength, 0,
                    origin.z + offsetXZ.y  * 2 * biomeLength);
                var newBiomePoint4 = new Vector3Int(origin.x + offsetXZ.x * 2 * biomeLength, 0,
                    origin.z + offsetXZ.y * 2 * biomeLength);

                //Duplicates are not added because its a hashmap
                biomeCentersTemp.Add(newBiomePoint1);
                biomeCentersTemp.Add(newBiomePoint2);
                biomeCentersTemp.Add(newBiomePoint3);
                biomeCentersTemp.Add(newBiomePoint4);
            }

            return new List<Vector3Int>(biomeCentersTemp);
        }
    }
}