// Copyright 2020-2021 Miyakowww.
//
// 此源代码的使用受 GNU AFFERO GENERAL PUBLIC LICENSE version 3 许可证的约束, 可以在以下链接找到该许可证.
// Use of this source code is governed by the GNU AGPLv3 license that can be found through the following link.
//
// https://github.com/Miyakowww/CocoaFramework/blob/main/LICENSE

using CocoaFramework.Core;
using CocoaFramework.Support;
using Mirai_CSharp;
using Mirai_CSharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CocoaFramework.Docking
{
    public static class BotStartup
    {
        private static bool connecting = false;
        private static readonly MiraiHttpSession session = new MiraiHttpSession();

        public static async Task<bool> Start(BotStartupConfig config)
        {
            if (connecting || session.Connected)
            {
                return false;
            }
            else
            {
                connecting = true;
            }
            MiraiHttpSessionOptions options = new MiraiHttpSessionOptions(config.host, config.port, config.authKey);
            try
            {
                await session.ConnectAsync(options, config.qqID);
            }
            catch
            {
                connecting = false;
                return false;
            }
            if (session.Connected)
            {
                session.AddPlugin(new MainPlugin());
                await BotCore.Init(session, config);
                connecting = false;
                return true;
            }
            else
            {
                connecting = false;
                return false;
            }
        }

        public static async ValueTask Dispose()
        {
            while (connecting)
            {
                await Task.Delay(10);
            }
            BotCore.AutoSave = false;
            foreach (var w in MiddlewareCore.Middlewares)
            {
                w.SaveData();
            }
            foreach (var m in ModuleCore.Modules)
            {
                m.SaveData();
            }
            foreach (var s in ServiceCore.Services)
            {
                s.SaveData();
            }
            foreach (var c in ComponentCore.Components)
            {
                c.SaveData();
            }
            while (DataManager.SavingData)
            {
                await Task.Delay(10);
            }
            await session.DisposeAsync();
        }
    }

    public class BotStartupConfig
    {
        public string host;
        public int port;
        public string authKey;
        public long qqID;

        public List<BotMiddlewareBase> Middlewares { get; } = new();
        public Assembly assembly;
        public TimeSpan autoSave;

        public BotStartupConfig(string host, int port, string authKey, long qqID)
        {
            this.host = host;
            this.port = port;
            this.authKey = authKey;
            this.qqID = qqID;
            assembly = Assembly.GetEntryAssembly()!;
            autoSave = TimeSpan.FromMinutes(5);
        }

        public BotStartupConfig AddMiddleware(BotMiddlewareBase mw)
        {
            Middlewares.Add(mw);
            return this;
        }
    }
}
