namespace Voxel_Engine.WorldGen.ChunkFeatureGenerator
{
    public interface IChunkFeatureGenerator
    {
        public void Handle(ChunkData chunkData);
    }
}