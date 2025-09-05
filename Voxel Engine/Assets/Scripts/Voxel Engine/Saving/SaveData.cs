using System;

namespace Voxel_Engine.Saving
{
    [Serializable]
    public abstract class SaveData
    {
        public void Save(string fileName)
        {
            SaveSystem.SaveData(this, fileName);
        }

        public virtual T Load<T>(string fileName) where T : SaveData
        {
            return SaveSystem.LoadData<T>(fileName);
        }
    }
}