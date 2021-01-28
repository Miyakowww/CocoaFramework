using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CocoaFramework.Support
{
    public static class DataManager
    {
        private static readonly ConcurrentDictionary<string, bool> needSave = new();
        private static readonly ConcurrentDictionary<string, bool> saving = new();
        private static int savingCount = 0;

        internal static bool SavingData => savingCount > 0;

        public static readonly string dataPath = AppDomain.CurrentDomain.BaseDirectory + "/data/";

        public static async Task SaveData(string name, object? obj)
        {
            if (obj is null)
            {
                return;
            }
            if (saving.GetOrAdd(name, false))
            {
                needSave[name] = true;
            }
            else
            {
                saving[name] = true;
                Interlocked.Increment(ref savingCount);
                string path = $@"{dataPath}{name}.json";
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                }
                await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(obj, Formatting.Indented));
                while (needSave.GetOrAdd(name, false))
                {
                    needSave[name] = false;
                    await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(obj, Formatting.Indented));
                }
                saving[name] = false;
                Interlocked.Decrement(ref savingCount);
            }
        }
        public static async Task<T?> LoadData<T>(string name)
        {
            while (needSave.GetOrAdd(name, false) || saving.GetOrAdd(name, false))
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
            while (needSave.GetOrAdd(name, false) || saving.GetOrAdd(name, false))
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