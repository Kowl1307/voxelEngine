using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;

namespace Voxel_Engine.Saving
{
    public static class SaveSystem
    {
        private static readonly ConcurrentQueue<Task> _savingTasks = new ();
        private static Task _currentSavingTask;
        
        private static readonly object Lock = new();
        private static bool _isProcessing = false;
        
        public static void SaveData<T>(T data, string filePath) where T : class
        {
            var completePath = Application.persistentDataPath + "/" + filePath;
            
            _savingTasks.Enqueue(SaveTask(data, completePath));
            ProcessQueue();
        }

        private static async void ProcessQueue()
        {
            lock (Lock)
            {
                if (_isProcessing)
                    return;

                _isProcessing = true;
            }

            while (true)
            {
                if (!_savingTasks.TryDequeue(out var nextTask))
                    break;

                _currentSavingTask = nextTask;
                await _currentSavingTask;
            }

            lock (Lock)
            {
                _isProcessing = false;
            }
        }

        private static async Task SaveTask<T>(T data, string completePath) where T : class
        {
            await Task.Run(() =>
            {
                var directoryPath = Path.GetDirectoryName(completePath);

                if (!Directory.Exists(directoryPath))
                {
                    if (directoryPath != null) Directory.CreateDirectory(directoryPath);
                }
                /*var formatter = new BinaryFormatter();
                using var fileStream = new FileStream(completePath, FileMode.OpenOrCreate);
                formatter.Serialize(fileStream, data);
                */
                File.WriteAllTextAsync(completePath, JsonUtility.ToJson(data));
            });
        }

        public static T LoadData<T>(string filePath) where T : class
        {
            var completePath = Application.persistentDataPath + "/" + filePath;
            if (File.Exists(completePath))
            {
                var formatter = new BinaryFormatter();
                var fileStream = new FileStream(completePath, FileMode.Open);
                
                var data = formatter.Deserialize(fileStream) as T;
                
                return data;
            }
            
            Debug.LogError("Save-File not found: " + completePath);
            throw new FileNotFoundException("Save-File not found: " + completePath);
        }
    }
}