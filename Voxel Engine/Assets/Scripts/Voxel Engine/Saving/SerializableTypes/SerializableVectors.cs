using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace Voxel_Engine.Saving.SerializableTypes
{
    
    [Serializable]
    public class SerializableVector3 : ISerializationCallbackReceiver
    {
        public float[] values;
        
        [NonSerialized] private Vector3 _vector3;

        public SerializableVector3(Vector3 vector3)
        {
            _vector3 = vector3;
            values = new float[] { vector3.x, vector3.y, vector3.z };
        }

        public static implicit operator Vector3(SerializableVector3 serializableVector3)
        {
            return serializableVector3._vector3;
        }

        public static implicit operator SerializableVector3(Vector3 serializableVector3)
        {
            return new SerializableVector3(serializableVector3);
        }

        public void OnBeforeSerialize()
        {
            values = new float[] { _vector3.x, _vector3.y, _vector3.z };
        }

        public void OnAfterDeserialize()
        {
            Debug.Assert(values.Length == 3);
            _vector3 = new Vector3(values[0], values[1], values[2]);
        }
    }
    
    [Serializable]
    public class SerializableVector3Int : ISerializationCallbackReceiver
    {
        public int[] values;
        
        [NonSerialized] private Vector3Int _vector3;

        public SerializableVector3Int(Vector3Int vector3)
        {
            _vector3 = vector3;
            values = new int[] { vector3.x, vector3.y, vector3.z };
        }

        public static implicit operator Vector3Int(SerializableVector3Int serializableVector3)
        {
            return serializableVector3._vector3;
        }

        public static implicit operator SerializableVector3Int(Vector3Int serializableVector3)
        {
            return new SerializableVector3Int(serializableVector3);
        }

        public void OnBeforeSerialize()
        {
            values = new int[] { _vector3.x, _vector3.y, _vector3.z };
        }

        public void OnAfterDeserialize()
        {
            Debug.Assert(values.Length == 3);
            _vector3 = new Vector3Int(values[0], values[1], values[2]);
        }
    }
}