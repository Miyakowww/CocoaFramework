﻿using CocoaFramework.Model;
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

        [Obsolete("请不要手动进行初始化")]
        public static void Init(BotMiddlewareBase[] middlewares)
        {
            Middlewares = ImmutableArray.Create(middlewares);

            foreach (var m in Middlewares)
            {
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
    }
}
