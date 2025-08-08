using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Voxel_Engine.WorldGen.Noise
{
    public static class DataProcessing
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

        public static List<Vector2Int> FindLocalMaxima(float[,] dataMatrix, int range = 1)
        {
            var maximums = new List<Vector2Int>();
            for (var x = 0; x < dataMatrix.GetLength(0); x++)
            {
                for (var z = 0; z < dataMatrix.GetLength(1); z++)
                {
                    var noiseVal = dataMatrix[x, z];
                    if (CheckNeighbours(dataMatrix, x, z, range, (neighbourVal) => neighbourVal < noiseVal))
                    {
                        maximums.Add(new Vector2Int(x, z));
                    }
                }                
            }

            return maximums;
        }

        private static bool CheckNeighbours(float[,] dataMatrix, int x, int y, Func<float, bool> successCondition)
        {
            foreach (var dir in directions)
            {
                var newPos = new Vector2Int(x + dir.x, y + dir.y);

                if (newPos.x < 0 || newPos.x >= dataMatrix.GetLength(0) || newPos.y < 0 ||
                    newPos.y >= dataMatrix.GetLength(1))
                    continue;

                if (successCondition(dataMatrix[x + dir.x, y + dir.y]) == false)
                    return false;
            }

            return true;
        }

        private static bool CheckNeighbours(float[,] dataMatrix, int x, int y, int range, Func<float, bool> successCondition)
        {
            var neighbourValues = GetNeighboursInRange(dataMatrix, x, y, range);
            return neighbourValues.All(successCondition);
        }
        private static List<float> GetNeighboursInRange(float[,] dataMatrix, int x, int y, int range)
        {
            var neighbours = new List<float>();

            for (var xOffset = -range; xOffset < range; xOffset++)
            {
                for (var yOffset = -range; yOffset < range; yOffset++)
                {
                    // Dont add center
                    if (xOffset == 0 && yOffset == 0) continue;
                    
                    var newPos = new Vector2Int(x + xOffset, y + yOffset);
                    
                    if (newPos.x < 0 || newPos.x >= dataMatrix.GetLength(0) || newPos.y < 0 ||
                        newPos.y >= dataMatrix.GetLength(1))
                        continue;
                    
                    neighbours.Add(dataMatrix[newPos.x, newPos.y]);
                }
            }

            return neighbours;
        }
    }
}