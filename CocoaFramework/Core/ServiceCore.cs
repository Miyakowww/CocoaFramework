using CocoaFramework.Model;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CocoaFramework.Core
{
    public static class ServiceCore
    {
        public static ImmutableArray<BotServiceBase> Services { get; private set; }

        [Obsolete("请不要手动进行初始化")]
        public static void Init(Assembly assembly)
        {
            List<BotServiceBase> services = new();
            Type[] types = assembly.GetTypes();
            foreach (var t in types)
            {
                if (t.BaseType == typeof(BotServiceBase))
                {
                    DisabledAttribute? d = t.GetCustomAttribute<DisabledAttribute>();
                    if (d is not null)
                    {
                        continue;
                    }
                    BotServiceAttribute? s = t.GetCustomAttribute<BotServiceAttribute>();
                    if (s is not null)
                    {
                        BotServiceBase? service = Activator.CreateInstance(t) as BotServiceBase;
                        if (service is not null)
                        {
                            service.Init();
                            services.Add(service);
                        }
                    }
                }
            }
            Services = ImmutableArray.Create(services.ToArray());
        }

        [Obsolete("请不要手动调用此方法")]
        public static void Run(MessageSource source, QMessage msg, QMessage origMsg, bool processed, BotModuleBase? processModule)
        {
            foreach (var s in Services)
            {
                s.Run(source, msg, origMsg, processed, processModule);
            }
        }
    }

    public abstract class BotServiceBase
    {
        public virtual void Init() { }
        public virtual void Run(MessageSource source, QMessage msg, QMessage origMsg, bool processed, BotModuleBase? processModule) { }
    }
}
