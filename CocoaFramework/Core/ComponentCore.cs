using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CocoaFramework.Core
{
    public static class ComponentCore
    {
        public static ImmutableArray<BotComponentBase> Components { get; private set; }

        [Obsolete("请不要手动进行初始化")]
        public static void Init(Assembly assembly)
        {
            List<BotComponentBase> components = new();
            Type[] types = assembly.GetTypes();
            foreach (var t in types)
            {
                if (t.BaseType == typeof(BotComponentBase))
                {
                    DisabledAttribute? d = t.GetCustomAttribute<DisabledAttribute>();
                    if (d is not null)
                    {
                        continue;
                    }
                    BotComponentAttribute? s = t.GetCustomAttribute<BotComponentAttribute>();
                    if (s is not null)
                    {
                        BotComponentBase? component = Activator.CreateInstance(t) as BotComponentBase;
                        if (component is not null)
                        {
                            component.Init();
                            components.Add(component);
                        }
                    }
                }
            }
            Components = ImmutableArray.Create(components.ToArray());
        }
    }

    public abstract class BotComponentBase
    {
        public virtual void Init() { }
    }
}
