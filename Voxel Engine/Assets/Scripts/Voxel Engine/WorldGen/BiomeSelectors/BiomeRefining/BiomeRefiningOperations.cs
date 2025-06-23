using Unity.Mathematics.Geometry;
using UnityEngine;
using Voxel_Engine.WorldGen.Noise;
using static System.Math;


namespace Voxel_Engine.WorldGen.BiomeSelectors.BiomeRefining
{
    public static class BiomeRefiningOperations
    {
        public static ResolutionMap NearestNeighborZoom(this ResolutionMap resMap, float zoomFactor)
        {
            // Neue Auflösung berechnen (kleiner, da mehr Details pro Pixel)
            var newResolution = (int)Max(1, resMap.Resolution / zoomFactor);

            // Neue Map-Größe berechnen (größer, da mehr Pixel)
            var oldWidth = resMap.Map.GetLength(0);
            var oldHeight = resMap.Map.GetLength(1);

            var newWidth = (int)Round(oldWidth * zoomFactor);
            var newHeight = (int)Round(oldHeight * zoomFactor);

            var newMap = new Color[newWidth, newHeight];

            // Einfache nearest-neighbor Skalierung
            for (var x = 0; x < newWidth; x++)
            {
                for (var y = 0; y < newHeight; y++)
                {
                    var oldX = (int)(x / zoomFactor);
                    var oldY = (int)(y / zoomFactor);

                    // Begrenzung auf gültigen Bereich der alten Map
                    oldX = Min(oldX, oldWidth - 1);
                    oldY = Min(oldY, oldHeight - 1);

                    newMap[x, y] = resMap.Map[oldX, oldY];
                }
            }

            return new ResolutionMap
            {
                Resolution = newResolution,
                Map = newMap
            };
        }
        
        //TODO
        public static ResolutionMap ImperfectZoom(this ResolutionMap resMap, float zoomFactor, NoiseSettings noiseSettings)
        {
            // Neue Auflösung berechnen (kleiner, da mehr Details pro Pixel)
            var newResolution = (int)Max(1, resMap.Resolution / zoomFactor);

            // Neue Map-Größe berechnen (größer, da mehr Pixel)
            var oldWidth = resMap.Map.GetLength(0);
            var oldHeight = resMap.Map.GetLength(1);

            var newWidth = (int)Round(oldWidth * zoomFactor);
            var newHeight = (int)Round(oldHeight * zoomFactor);

            var newMap = new Color[newWidth, newHeight];

            // Einfache nearest-neighbor Skalierung
            for (var x = 0; x < newWidth; x++)
            {
                for (var y = 0; y < newHeight; y++)
                {
                    var oldX = (int)(x / zoomFactor);
                    var oldY = (int)(y / zoomFactor);

                    // Begrenzung auf gültigen Bereich der alten Map
                    oldX = Min(oldX, oldWidth - 1);
                    oldY = Min(oldY, oldHeight - 1);

                    var pseudoRandom = Mathf.PerlinNoise((float)oldX / oldWidth, (float)oldY / oldHeight);
                    if (pseudoRandom < 0.2)
                    {
                        newMap[x, y] = Color.blue;
                    }
                    else if (pseudoRandom < 0.4)
                    {
                        newMap[x, y] = Color.green;
                    }
                    else
                    {
                        newMap[x, y] = resMap.Map[oldX, oldY];
                    }
                }
            }

            return new ResolutionMap
            {
                Resolution = newResolution,
                Map = newMap
            };
        }

        public static ResolutionMap IncreaseLandmass(this ResolutionMap resMap)
        {
            var newMap = resMap;
            var n = resMap.Map.GetLength(0);
            var green = Color.green;

            // Hilfsarray, um zu speichern, welche Pixel geändert werden sollen
            var toGreen = new bool[n][];
            for (var index = 0; index < n; index++)
            {
                toGreen[index] = new bool[n];
            }

            // Schritt 1: Prüfe alle Pixel und merke, welche geändert werden sollen
            for (var y = 0; y < n; y++)
            {
                for (var x = 0; x < n; x++)
                {
                    var greenNeighbors = 0;

                    if (y > 0 && resMap.Map[x, y - 1] == green) greenNeighbors++;
                    if (y < n - 1 && resMap.Map[x, y + 1] == green) greenNeighbors++;
                    if (x > 0 && resMap.Map[x - 1, y] == green) greenNeighbors++;
                    if (x < n - 1 && resMap.Map[x + 1, y] == green) greenNeighbors++;

                    if (greenNeighbors >= 2)
                        toGreen[x][y] = true;
                }
            }

            // Schritt 2: Setze die markierten Pixel auf grün
            for (var y = 0; y < n; y++)
            {
                for (var x = 0; x < n; x++)
                {
                    if (toGreen[x][y])
                        newMap.Map[x, y] = green;
                }
            }

            return newMap;
        }
    }
}