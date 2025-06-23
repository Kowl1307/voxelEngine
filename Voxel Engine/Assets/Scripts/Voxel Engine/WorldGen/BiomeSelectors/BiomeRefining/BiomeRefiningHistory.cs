using System.Collections.Generic;

namespace Voxel_Engine.WorldGen.BiomeSelectors.BiomeRefining
{
    public class BiomeRefiningHistory
    {
        private readonly List<ResolutionMap> _history = new ();

        public void AddResolutionMap(ResolutionMap resolutionMap)
        {
            _history.Add(resolutionMap);
        }

        public ResolutionMap GetResolutionMap(int resolution)
        {
            return _history.Find(resMap => resMap.Resolution == resolution);
        }

        public ResolutionMap GetResolutionMapByIndex(int index)
        {
            return _history[index];
        }

        public int GetHistoryDepth()
        {
            return _history.Count;
        }
    }
}