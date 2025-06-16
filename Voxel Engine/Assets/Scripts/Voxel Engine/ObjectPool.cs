using System.Collections.Generic;
using UnityEngine;

namespace Voxel_Engine
{
    public class ObjectPool<T> : Object where T : MonoBehaviour
    {
        private readonly Queue<T> _objects;
        private readonly T _objectPrefab;

        private int _refillAmount = 4;

        public ObjectPool(T prefab)
        {
            _objects = new Queue<T>();
            _objectPrefab = prefab;
        }

        public void FillTo(int capacity)
        {
            for (var i = _objects.Count; i < capacity; i++)
            {
                var newChunk = Instantiate(_objectPrefab, Vector3.zero, Quaternion.identity);
                //newChunk.gameObject.SetActive(false);
                _objects.Enqueue(newChunk);
            }
        }

        public T GetObject()
        {
            if (_objects.Count <= 0)
            {
                FillTo(_refillAmount);
            }
            
            return _objects.Dequeue();
        }

        public void ReturnObject(T obj)
        {
            _objects.Enqueue(obj);
        }

        public void Clear()
        {
            _objects.Clear();
        }

        public void SetRefillAmount(int amount)
        {
            _refillAmount = amount;
        }
    }
}