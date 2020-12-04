using CocoaFramework.Model;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoaFramework.Core
{
    public static class MiddlewareCore
    {
        public static ImmutableArray<BotMiddlewareBase> Middlewares { get; private set; }

        public static void Init(params BotMiddlewareBase[] middlewares)
        {
            Middlewares = ImmutableArray.Create(middlewares);

            foreach (var m in Middlewares)
            {
                m.Init();
            }
        }

        [Obsolete("请不要手动调用此方法")]
        public static bool Run(ref MessageSource source, ref QMessage msg)
        {
            foreach (var m in Middlewares)
            {
                bool ctn = m.Run(ref source, ref msg);
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
        public abstract bool Run(ref MessageSource source, ref QMessage msg);
    }
}
