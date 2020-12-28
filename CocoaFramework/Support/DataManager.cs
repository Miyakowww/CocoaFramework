using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CocoaFramework.Support
{
    public static class DataManager
    {
        private static readonly List<string> needSave = new();
        private static readonly List<string> saving = new();

        internal static bool SavingData => needSave.Count > 0 || saving.Count > 0;

        public static readonly string dataPath = AppDomain.CurrentDomain.BaseDirectory + @"\data\";

        public static async Task SaveData(string name, object? obj)
        {
            if (obj is null)
            {
                return;
            }
            if (saving.Contains(name))
            {
                if (!needSave.Contains(name))
                {
                    needSave.Add(name);
                }
            }
            else
            {
                saving.Add(name);
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
                await Task.Delay(10);
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
                await Task.Delay(10);
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
    }
}