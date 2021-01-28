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
    public static class ComponentCore
    {
        public static ImmutableArray<BotComponentBase> Components { get; private set; }

        internal static void Init(Assembly assembly)
        {
            List<BotComponentBase> components = new();
            Type[] types = assembly.GetTypes();
            foreach (var t in types)
            {
                if (t.BaseType != typeof(BotComponentBase)) // 提前判断，避免不必要的实例化
                {
                    continue;
                }
                if (t.GetCustomAttribute<DisabledAttribute>() is not null)
                {
                    continue;
                }
                if (t.GetCustomAttribute<BotComponentAttribute>() is null)
                {
                    continue;
                }
                if (Activator.CreateInstance(t) is not BotComponentBase component)
                {
                    continue;
                }
                component.InitData();
                component.Init();
                components.Add(component);
            }
            Components = components.ToImmutableArray();
        }
    }

    public abstract class BotComponentBase
    {
        protected internal virtual void Init() { }

        private readonly List<FieldInfo> hostedFields = new();
        private string? TypeName;

        internal void InitData()
        {
            foreach (var f in GetType().GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (f.GetCustomAttributes<HostedDataAttribute>().Any())
                {
                    hostedFields.Add(f);
                }
            }
            TypeName = GetType().Name;
            LoadData();
        }
        internal void LoadData()
        {
            if (!Directory.Exists($@"{DataManager.dataPath}ComponentData/{TypeName}"))
            {
                Directory.CreateDirectory($@"{DataManager.dataPath}ComponentData/{TypeName}");
            }
            foreach (var f in hostedFields)
            {
                object? val = DataManager.LoadData($@"ComponentData/{TypeName}/Field_{f.Name}", f.FieldType).Result;
                if (val is not null)
                {
                    f.SetValue(this, val);
                }
            }
        }
        internal void SaveData()
        {
            foreach (var f in hostedFields)
            {
                _ = DataManager.SaveData($@"ComponentData/{TypeName}/Field_{f.Name}", f.GetValue(this));
            }
        }
    }
}
