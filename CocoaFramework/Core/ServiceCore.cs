﻿using CocoaFramework.Model;
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
    public static class ServiceCore
    {
        public static ImmutableArray<BotServiceBase> Services { get; private set; }

        internal static void Init(Assembly assembly)
        {
            List<BotServiceBase> services = new();
            Type[] types = assembly.GetTypes();
            foreach (var t in types)
            {
                if (t.BaseType != typeof(BotServiceBase))
                {
                    continue;
                }
                DisabledAttribute? d = t.GetCustomAttribute<DisabledAttribute>();
                if (d is not null)
                {
                    continue;
                }
                BotServiceAttribute? s = t.GetCustomAttribute<BotServiceAttribute>();
                if (s is null)
                {
                    continue;
                }
                BotServiceBase? service = Activator.CreateInstance(t) as BotServiceBase;
                if (service is not null)
                {
                    service.InitData();
                    service.Init();
                    services.Add(service);
                }
            }
            Services = ImmutableArray.Create(services.ToArray());
        }

        internal static void Run(MessageSource src, QMessage msg, QMessage origMsg, bool processed, BotModuleBase? processModule)
        {
            foreach (var s in Services)
            {
                s.Run(src, msg, origMsg, processed, processModule);
            }
        }
    }

    public abstract class BotServiceBase
    {
        protected internal virtual void Init() { }
        protected internal abstract void Run(MessageSource src, QMessage msg, QMessage origMsg, bool processed, BotModuleBase? processModule);

        private readonly List<FieldInfo> fields = new();
        private string? TypeName;

        internal void InitData()
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
        internal void LoadData()
        {
            if (!Directory.Exists($@"{DataManager.dataPath}ServiceData\{TypeName}"))
            {
                Directory.CreateDirectory($@"{DataManager.dataPath}ServiceData\{TypeName}");
            }
            foreach (var f in fields)
            {
                object? val = DataManager.LoadData($@"ServiceData\{TypeName}\Field_{f.Name}", f.FieldType).Result;
                if (val is not null)
                {
                    f.SetValue(this, val);
                }
            }
        }
        internal void SaveData()
        {
            foreach (var f in fields)
            {
                _ = DataManager.SaveData($@"ServiceData\{TypeName}\Field_{f.Name}", f.GetValue(this));
            }
        }
    }
}
