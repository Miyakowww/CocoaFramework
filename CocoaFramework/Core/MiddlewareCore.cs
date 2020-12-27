using CocoaFramework.Model;
using CocoaFramework.Support;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CocoaFramework.Core
{
    public static class MiddlewareCore
    {
        public static ImmutableArray<BotMiddlewareBase> Middlewares { get; private set; }

        [Obsolete("请不要手动进行初始化")]
        public static void Init(BotMiddlewareBase[] middlewares)
        {
            Middlewares = ImmutableArray.Create(middlewares);

            foreach (var m in Middlewares)
            {
                m.InitData();
                m.Init();
            }
        }

        [Obsolete("请不要手动调用此方法")]
        public static bool Run(ref MessageSource src, ref QMessage msg)
        {
            foreach (var m in Middlewares)
            {
                bool ctn = m.Run(ref src, ref msg);
                if (!ctn)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public abstract class BotMiddlewareBase
    {
        public virtual void Init() { }
        public abstract bool Run(ref MessageSource src, ref QMessage msg);

        private readonly List<FieldInfo> fields = new();
        private string? TypeName;

        public void InitData()
        {
            foreach (var f in GetType().GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (f.GetCustomAttributes<HostedDataAttribute>().Any())
                {
                    fields.Add(f);
                }
            }
            TypeName = GetType().Name;
            LoadData();
        }
        public void LoadData()
        {
            if (!Directory.Exists($@"{DataManager.dataPath}MiddlewareData\{TypeName}"))
            {
                Directory.CreateDirectory($@"{DataManager.dataPath}MiddlewareData\{TypeName}");
            }
            foreach (var f in fields)
            {
                object? val = DataManager.LoadData($@"MiddlewareData\{TypeName}\Field_{f.Name}", f.FieldType).Result;
                if (val is not null)
                {
                    f.SetValue(this, val);
                }
            }
        }
        public void SaveData()
        {
            foreach (var f in fields)
            {
                _ = DataManager.SaveData($@"MiddlewareData\{TypeName}\Field_{f.Name}", f.GetValue(this));
            }
        }
    }
}
