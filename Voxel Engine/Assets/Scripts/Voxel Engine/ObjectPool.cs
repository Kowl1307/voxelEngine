using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Voxel_Engine
{
    public class ObjectPool<T> : Object where T : UnityEngine.Object
    {
        private readonly object _lock = new object();
        
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
            lock (_lock)
            {
                for (var i = _objects.Count; i < capacity; i++)
                {
                    var newObject = Instantiate(_objectPrefab, Vector3.zero, Quaternion.identity);
                    _objects.Enqueue(newObject);
                }
            }
        }

        public async Task FillToAsync(int capacity)
        {
            var refillAmount = 0;
            lock (_lock)
            {
                refillAmount = capacity - _objects.Count;
            }
            
            for (var i = 0; i < refillAmount; i++)
            {
                var newObject = await UnityMainThreadDispatcher.Instance()
                    .EnqueueAsync(() => Instantiate(_objectPrefab, Vector3.zero, Quaternion.identity));

                lock (_lock)
                {
                    _objects.Enqueue(newObject);
                }
            }
        }

        public void AddAmount(int amount)
        {
            for (var i = 0; i < amount; i++)
            {
                var newObject = Instantiate(_objectPrefab, Vector3.zero, Quaternion.identity);
                lock (_lock)
                {
                    _objects.Enqueue(newObject);
                }
            }
        }

        public T GetObject()
        {
            lock (_lock)
            {
                if (_objects.Count <= 0)
                {
                    FillTo(_refillAmount);
                }
                
                return _objects.Dequeue();
            }
        }

        public async Task<T> GetObjectAsync()
        {
            lock (_lock)
            {
                if(_objects.Count > 0)
                    return _objects.Dequeue();
            }

            await FillToAsync(_refillAmount);

            lock (_lock)
            {
                return _objects.Dequeue();
            }
        }

        public void ReturnObject(T obj)
        {
            lock (_lock)
            {
                _objects.Enqueue(obj);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _objects.Clear();
            }
        }

        public void SetRefillAmount(int amount)
        {
            _refillAmount = amount;
        }

        public int CurrentAmount()
        {
            lock (_lock)
            {
                return _objects.Count;
            }
        }
    }
}