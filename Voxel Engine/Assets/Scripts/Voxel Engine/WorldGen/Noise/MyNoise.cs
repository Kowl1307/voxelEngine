using Unity.Mathematics;
using UnityEngine;

namespace Voxel_Engine.WorldGen.Noise
{
    public static class MyNoise
    {
        public static float RemapValue(float value, float initMin, float initMax, float outMin, float outMax)
        {
            return outMin + (value - initMin) * (outMax - outMin) / (initMax - initMin);
        }

        public static float RemapValue01(float value, float outMin, float outMax)
        {
            return RemapValue(value, 0, 1, outMin, outMax);
        }

        public static int RemapValue01ToInt(float value, float outMin, float outMax)
        {
            return (int)RemapValue01(value, outMin, outMax);
        }

        public static float Redistribution(float noise, NoiseSettings settings)
        {
            return Mathf.Pow(noise * settings.RedistributionModifier, settings.Exponent);
        }
        
        
        //https://adrianb.io/2014/08/09/perlinnoise.html
        public static float OctavePerlin(float x, float z, NoiseSettings settings)
        {
            x *= settings.NoiseZoom;
            z *= settings.NoiseZoom;
            x += settings.NoiseZoom;
            z += settings.NoiseZoom;

            float total = 0;
            float frequency = 1;
            float amplitude = 1;
            float amplitudeSum = 0;  // Used for normalizing result to 0.0 - 1.0 range
            for (var i = 0; i < settings.Octaves; i++)
            {
                total += Mathf.PerlinNoise((settings.Offset.x + settings.WorldOffset.x + x) * frequency, (settings.Offset.y + settings.WorldOffset.y + z) * frequency) * amplitude;

                amplitudeSum += amplitude;

                amplitude *= settings.Persistance;
                frequency *= 2;
            }

            return total / amplitudeSum;
        }

        public static float SimplexNoise(float x, float z, NoiseSettings settings)
        {
            return RemapValue(noise.snoise(new float2((settings.Offset.x + settings.WorldOffset.x + x),
                (settings.Offset.y + settings.WorldOffset.y + z))), -1, 1, 0, 1);
        }

        public static float OctaveSimplex(float x, float z, NoiseSettings settings)
        {
            x *= settings.NoiseZoom;
            z *= settings.NoiseZoom;
            x += settings.NoiseZoom;
            z += settings.NoiseZoom;

            float total = 0;
            float frequency = 1;
            float amplitude = 1;
            float amplitudeSum = 0;  // Used for normalizing result to 0.0 - 1.0 range
            for (var i = 0; i < settings.Octaves; i++)
            {
                total += SimplexNoise(x,z,settings) * amplitude;
                amplitudeSum += amplitude;

                amplitude *= settings.Persistance;
                frequency *= 2;
            }
            
            if(total < 0)
                Debug.Log("Total:"+total);
            return total / amplitudeSum;
        }
    }
}
