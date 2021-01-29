// Copyright 2020-2021 Miyakowww.
//
// 此源代码的使用受 GNU AFFERO GENERAL PUBLIC LICENSE version 3 许可证的约束, 可以在以下链接找到该许可证.
// Use of this source code is governed by the GNU AGPLv3 license that can be found through the following link.
//
// https://github.com/Miyakowww/CocoaFramework/blob/main/LICENSE

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