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

        public static async Task<bool> Start(string host, int port, string authKey, long qqID)
        {
            if (connecting)
            {
                return false;
            }
            else
            {
                connecting = true;
            }
            MiraiHttpSessionOptions options = new MiraiHttpSessionOptions(host, port, authKey);
            try
            {
                await session.ConnectAsync(options, qqID);
            }
            catch
            {
                return false;
            }
            if (session.Connected)
            {
                session.AddPlugin(new MainPlugin());
                Assembly userAssembly = Assembly.GetEntryAssembly()!;
                await BotCore.Init(session, userAssembly);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static ValueTask Dispose()
        {
            return session.DisposeAsync();
        }
    }
}
