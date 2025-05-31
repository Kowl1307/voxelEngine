using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using Voxel_Engine.WorldGen.Noise;
using Random = System.Random;

namespace Voxel_Engine.WorldGen.Biomes.BiomeRefining
{
    public class BiomeRefining
    {
        struct ResolutionMap
        {
            public int Resolution;
            public Color[,] Map;
        }
        
        public float Test(int x, int y, NoiseSettings noiseSettings)
        {
            var random = new Random(noiseSettings.Seed.x);
            for (var i = 0; i < x + y; i++) random.NextDouble();
            return (float)random.NextDouble();
        }

        public Color GetBiomeAt(int x, int y, NoiseSettings noiseSettings)
        {
            var creationHistory = CreateRefinedMap(4096, 200, 200, noiseSettings);
            return creationHistory[^1][x,y];
        }

        /// <summary>
        /// Creates a 1:1 map for biomes
        /// Returns an array of size startWidth * startZoom, startHeight * startZoom
        /// </summary>
        /// <param name="startResolution">How many blocks one pixel refers to</param>
        /// <param name="startWidth">Width in pixels</param>
        /// <param name="startHeight">Height in pixels</param>
        /// <returns>A list of the creation history</returns>
        public List<Color[,]> CreateRefinedMap(int startResolution, int startWidth, int startHeight, NoiseSettings noiseSettings)
        {
            var history = new List<ResolutionMap>();
            var landOrOcean = new ResolutionMap()
                { Map = new Color[startWidth, startHeight], Resolution = startResolution };
            
            // Pseudorandomly set land or water
            var landProbabilty = 0.5;
            for (var x = 0; x < startWidth; x++)
            {
                for (var y = 0; y < startHeight; y++)
                {
                    if (SamplePseudoRandom(x, y, noiseSettings) <= landProbabilty)
                    {
                        landOrOcean.Map[x, y] = Color.green;
                    }
                    else
                    {
                        landOrOcean.Map[x, y] = Color.blue;
                    }
                }
            }
            
            history.Add(landOrOcean);

            return history.ConvertAll(resMap => resMap.Map);
        }

        private float SamplePseudoRandom(int x, int y, NoiseSettings noiseSettings)
        {
            var xMod = x ^ y + noiseSettings.Seed.x;
            var yMod = x+5 ^ y + noiseSettings.Seed.y;
            
            return Mathf.PerlinNoise(xMod, yMod);
        }

        private Color[,] Zoom(Color[,] map, float zoom)
        {
            return map;
        }
    }
}