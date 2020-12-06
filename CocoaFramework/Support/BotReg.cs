﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CocoaFramework.Support
{
    public static class BotReg
    {
        private static Dictionary<string, string> data = new();

        [Obsolete("请不要手动进行初始化")]
        public static void Init()
        {
            data = DataManager.LoadData<Dictionary<string, string>>("BotReg").Result ?? data;
        }

        private static void SaveData()
        {
            _ = DataManager.SaveData("BotReg", data);
        }

        public static bool Contains(string key)
            => data.ContainsKey(key);
        public static string[] GetKeys()
            => data.Keys.ToArray();
        public static string[] GetKeys(string path)
            => data.Keys
            .Where(k => k.StartsWith(path))
            .Select(k => k[path.Length..])
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
            if (data.ContainsKey(key))
            {
                data[key] = val;
            }
            else
            {
                data.Add(key, val);
            }
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
            if (data.ContainsKey(key))
            {
                data[key] = val.ToString();
            }
            else
            {
                data.Add(key, val.ToString());
            }
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
            if (data.ContainsKey(key))
            {
                data[key] = val.ToString();
            }
            else
            {
                data.Add(key, val.ToString());
            }
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
            if (data.ContainsKey(key))
            {
                data[key] = val.ToString();
            }
            else
            {
                data.Add(key, val.ToString());
            }
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
            if (data.ContainsKey(key))
            {
                data[key] = val.ToString();
            }
            else
            {
                data.Add(key, val.ToString());
            }
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
            if (data.ContainsKey(key))
            {
                data[key] = val.ToString();
            }
            else
            {
                data.Add(key, val.ToString());
            }
            SaveData();
        }

        public static bool Remove(string key)
        {
            if (Contains(key))
            {
                data.Remove(key);
                SaveData();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}