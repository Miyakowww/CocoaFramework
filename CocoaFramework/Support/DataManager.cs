using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CocoaFramework.Support
{
    public static class DataManager
    {
        private static readonly List<string> needSave = new List<string>();
        private static readonly List<string> saving = new List<string>();

        public static readonly string dataPath = AppDomain.CurrentDomain.BaseDirectory + @"\data\";

        public static async Task SaveData(string name, object? obj)
        {
            if (obj is null)
            {
                return;
            }
            if (saving.Contains(name))
            {
                needSave.UniqueAdd(name);
            }
            else
            {
                saving.UniqueAdd(name);
                string path = $@"{dataPath}{name}.json";
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                }
                await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(obj, Formatting.Indented));
                while (needSave.Contains(name))
                {
                    needSave.Remove(name);
                    await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(obj, Formatting.Indented));
                }
                saving.Remove(name);
            }
        }
        public static async Task<T?> LoadData<T>(string name)
        {
            while (needSave.Contains(name) || saving.Contains(name))
            {
                await Task.Delay(100);
            }
            if (File.Exists($@"{dataPath}{name}.json"))
            {
                return JsonConvert.DeserializeObject<T>(await File.ReadAllTextAsync($@"{dataPath}{name}.json"));
            }
            else
            {
                return default;
            }
        }
        public static async Task<object?> LoadData(string name, Type type)
        {
            while (needSave.Contains(name) || saving.Contains(name))
            {
                await Task.Delay(100);
            }
            if (File.Exists($@"{dataPath}{name}.json"))
            {
                return JsonConvert.DeserializeObject(await File.ReadAllTextAsync($@"{dataPath}{name}.json"), type);
            }
            else
            {
                return default;
            }
        }

        #region List Extensions
        public static void UniqueAdd<T>(this List<T> list, T item)
        {
            if (!list.Contains(item))
            {
                list.Add(item);
            }
        }
        #endregion
    }
}