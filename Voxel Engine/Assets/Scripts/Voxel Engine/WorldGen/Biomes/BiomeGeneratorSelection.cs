namespace Voxel_Engine.WorldGen.Biomes
{
    public class BiomeGeneratorSelection
    {
        public BiomeGenerator BiomeGenerator = null;
        public int? TerrainSurfaceNoise = null;

        public BiomeGeneratorSelection(BiomeGenerator biomeGenerator, int? terrainSurfaceNoise = null)
        {
            BiomeGenerator = biomeGenerator;
            TerrainSurfaceNoise = terrainSurfaceNoise;
        }
    }
}