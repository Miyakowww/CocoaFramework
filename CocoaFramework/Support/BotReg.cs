// Copyright 2020-2021 Miyakowww.
//
// 此源代码的使用受 GNU AFFERO GENERAL PUBLIC LICENSE version 3 许可证的约束, 可以在以下链接找到该许可证.
// Use of this source code is governed by the GNU AGPLv3 license that can be found through the following link.
//
// https://github.com/Miyakowww/CocoaFramework/blob/main/LICENSE

using System.Collections.Concurrent;
using System.Linq;

namespace CocoaFramework.Support
{
    public static class BotReg
    {
        private static ConcurrentDictionary<string, string> data = new();

        internal static void Init() 
            => data = DataManager.LoadData<ConcurrentDictionary<string, string>>("BotReg").Result ?? data;

        private static void SaveData() 
            => _ = DataManager.SaveData("BotReg", data);

        public static bool ContainsKey(string key)
            => data.ContainsKey(key);
        public static string[] GetKeys()
            => data.Keys.ToArray();
        public static string[] GetKeys(string path)
            => data.Keys
            .Where(k => k.StartsWith(path))
            .Select(k => k[(path.EndsWith('/') ? path.Length : path.Length + 1)..])
            .ToArray();

        public static string GetString(string key) => GetString(key, "");
        public static string GetString(string key, string defaultVal)
        {
            if (data.ContainsKey(key))
            {
                return data[key];
            }
            else
            {
                return defaultVal;
            }
        }
        public static void SetString(string key, string val)
        {
            data[key] = val;
            SaveData();
        }

        public static int GetInt(string key) => GetInt(key, 0);
        public static int GetInt(string key, int defaultVal)
        {
            if (data.ContainsKey(key))
            {
                if (int.TryParse(data[key], out int val))
                {
                    return val;
                }
                else
                {
                    return defaultVal;
                }
            }
            else
            {
                return defaultVal;
            }
        }
        public static void SetInt(string key, int val)
        {
            data[key] = val.ToString();
            SaveData();
        }

        public static long GetLong(string key) => GetLong(key, 0);
        public static long GetLong(string key, long defaultVal)
        {
            if (data.ContainsKey(key))
            {
                if (long.TryParse(data[key], out long val))
                {
                    return val;
                }
                else
                {
                    return defaultVal;
                }
            }
            else
            {
                return defaultVal;
            }
        }
        public static void SetLong(string key, long val)
        {
            data[key] = val.ToString();
            SaveData();
        }

        public static float GetFloat(string key) => GetFloat(key, 0);
        public static float GetFloat(string key, float defaultVal)
        {
            if (data.ContainsKey(key))
            {
                if (float.TryParse(data[key], out float val))
                {
                    return val;
                }
                else
                {
                    return defaultVal;
                }
            }
            else
            {
                return defaultVal;
            }
        }
        public static void SetFloat(string key, float val)
        {
            data[key] = val.ToString();
            SaveData();
        }

        public static double GetDouble(string key) => GetDouble(key, 0);
        public static double GetDouble(string key, double defaultVal)
        {
            if (data.ContainsKey(key))
            {
                if (double.TryParse(data[key], out double val))
                {
                    return val;
                }
                else
                {
                    return defaultVal;
                }
            }
            else
            {
                return defaultVal;
            }
        }
        public static void SetDouble(string key, double val)
        {
            data[key] = val.ToString();
            SaveData();
        }

        public static bool GetBool(string key) => GetBool(key, false);
        public static bool GetBool(string key, bool defaultVal)
        {
            if (data.ContainsKey(key))
            {
                if (bool.TryParse(data[key], out bool val))
                {
                    return val;
                }
                else
                {
                    return defaultVal;
                }
            }
            else
            {
                return defaultVal;
            }
        }
        public static void SetBool(string key, bool val)
        {
            data[key] = val.ToString();
            SaveData();
        }

        public static bool Remove(string key)
        {
            bool succeed = data.TryRemove(key, out _);
            SaveData();
            return succeed;
        }
    }
}