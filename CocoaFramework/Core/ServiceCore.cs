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
    public static class ServiceCore
    {
        public static ImmutableArray<BotServiceBase> Services { get; private set; }

        internal static void Init(Assembly assembly)
        {
            List<BotServiceBase> services = new();
            Type[] types = assembly.GetTypes();
            foreach (var t in types)
            {
                if (t.BaseType != typeof(BotServiceBase)) // 提前判断，避免不必要的实例化
                {
                    continue;
                }
                if (t.GetCustomAttribute<DisabledAttribute>() is not null)
                {
                    continue;
                }
                if (t.GetCustomAttribute<BotServiceAttribute>() is null)
                {
                    continue;
                }
                if (Activator.CreateInstance(t) is not BotServiceBase service)
                {
                    continue;
                }
                service.onMessageIsThreadSafe = t.GetMethod(nameof(BotServiceBase.OnMessage))?.GetCustomAttribute<ThreadSafeAttribute>() is not null;
                service.InitData();
                service.Init();
                services.Add(service);
            }
            Services = services.ToImmutableArray();
        }

        internal static void OnMessage(MessageSource src, QMessage msg, QMessage origMsg, bool processed, BotModuleBase? processModule)
        {
            foreach (var service in Services)
            {
                try
                {
                    service.OnMessageSafe(src, msg, origMsg, processed, processModule);
                }
                catch (Exception e)
                {
                    throw new Exception($"Service Run Error: {service.GetType()}", e);
                }
            }
        }
    }

    public abstract class BotServiceBase
    {
        protected internal virtual void Init() { }
        [ThreadSafe]
        protected internal virtual void OnMessage(MessageSource src, QMessage msg, QMessage origMsg, bool processed, BotModuleBase? processModule) { }

        private readonly List<FieldInfo> hostedFields = new();
        private string? TypeName;

        internal bool onMessageIsThreadSafe;
        internal readonly object onMessaeLock = new();

        internal void OnMessageSafe(MessageSource src, QMessage msg, QMessage origMsg, bool processed, BotModuleBase? processModule)
        {
            if (onMessageIsThreadSafe)
            {
                OnMessage(src, msg, origMsg, processed, processModule);
            }
            else
            {
                lock (onMessaeLock)
                {
                    OnMessage(src, msg, origMsg, processed, processModule);
                }
            }
        }

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
            if (!Directory.Exists($@"{DataManager.dataPath}ServiceData/{TypeName}"))
            {
                Directory.CreateDirectory($@"{DataManager.dataPath}ServiceData/{TypeName}");
            }
            foreach (var f in hostedFields)
            {
                object? val = DataManager.LoadData($@"ServiceData/{TypeName}/Field_{f.Name}", f.FieldType).Result;
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
                _ = DataManager.SaveData($@"ServiceData/{TypeName}/Field_{f.Name}", f.GetValue(this));
            }
        }
    }
}
