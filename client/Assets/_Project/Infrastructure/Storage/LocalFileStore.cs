using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace PlowParty.Infrastructure.Storage
{
    public sealed class LocalFileStore
    {
        private readonly string _folder;

        public LocalFileStore(string folder)
        {
            _folder = folder;
        }

        public bool TryRead<T>(string name, out T value)
        {
            value = default;
            var path = PathOf(name);
            if (!File.Exists(path))
            {
                return false;
            }

            try
            {
                value = JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
                return value != null;
            }
            catch (Exception exception) when (exception is IOException || exception is JsonException)
            {
                Debug.LogWarning($"Unreadable {path}: {exception.Message}");
                return false;
            }
        }

        public void Write<T>(string name, T value)
        {
            Directory.CreateDirectory(_folder);
            File.WriteAllText(PathOf(name), JsonConvert.SerializeObject(value));
        }

        private string PathOf(string name)
        {
            return Path.Combine(_folder, name + ".json");
        }
    }
}
