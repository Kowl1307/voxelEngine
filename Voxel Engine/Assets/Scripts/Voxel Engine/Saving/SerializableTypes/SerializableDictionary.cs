using System;
using System.Collections.Generic;
using UnityEngine;

namespace Voxel_Engine.Saving.SerializableTypes
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver
    {
        public List<TKey> keys = new List<TKey>();
        public List<TValue> values = new List<TValue>();

        [NonSerialized] public Dictionary<TKey, TValue> Dictionary = new Dictionary<TKey, TValue>();

        public static implicit operator Dictionary<TKey, TValue>(SerializableDictionary<TKey, TValue> dictionary)
        {
            return dictionary.Dictionary;
        }

        public static implicit operator SerializableDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            return new SerializableDictionary<TKey, TValue>()
            {
                Dictionary = dictionary
            };
        }
        
        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (var kvp in Dictionary)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            Dictionary = new Dictionary<TKey, TValue>();
            var count = Math.Min(keys.Count, values.Count);
            for (var i = 0; i < count; i++)
            {
                Dictionary[keys[i]] = values[i];
            }
        }
    }
}
