using CocoaFramework.Core;
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

        public BotStartupConfig(string host, int port, string authKey, long qqID)
        {
            this.host = host;
            this.port = port;
            this.authKey = authKey;
            this.qqID = qqID;
            assembly = Assembly.GetEntryAssembly()!;
        }
    }
}
