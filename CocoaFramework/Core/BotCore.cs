using CocoaFramework.Docking;
using CocoaFramework.Model;
using CocoaFramework.Support;
using Mirai_CSharp;
using Mirai_CSharp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CocoaFramework.Core
{
    internal static class BotCore
    {
        public static bool AutoSave = true;
        public static async Task Init(MiraiHttpSession session, BotStartupConfig config)
        {
            BotAPI.Init(session);

            BotReg.Init();
            BotAuth.Init();
            await BotInfo.ReloadAll();

            MiddlewareCore.Init(config.Middlewares.ToArray());
            ModuleCore.Init(config.assembly);
            ServiceCore.Init(config.assembly);
            ComponentCore.Init(config.assembly);

            _ = Task.Run(async () =>
            {
                TimeSpan delta = config.autoSave;
                while (AutoSave && delta > TimeSpan.Zero)
                {
                    await Task.Delay(config.autoSave);
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
                }
            });
        }

        public static void OnMessage(MessageSource src, QMessage msg)
        {
            try
            {
                QMessage newMsg = msg;

                bool release = MiddlewareCore.OnMessage(ref src, ref newMsg);
                if (release)
                {
                    int stat = ModuleCore.OnMessage(src, newMsg);
                    ServiceCore.OnMessage(src, newMsg, msg, stat != -1, stat >= 0 ? ModuleCore.Modules[stat] : null);
                }
            }
            catch (Exception e)
            {
                string einfo = $"\n{DateTime.Now}\nMessage:{(src.IsGroup ? $"[{src.group!.ID}]" : "")}[{src.user.ID}] {msg.PlainText}\n{e.StackTrace}\n{e.Message}";
                if (BotReg.GetBool("CORE/LOG_ERROR", true) && BotAuth.HasOwner)
                {
                    _ = BotAPI.SendPrivateMessageAsync(BotAuth.Owner, new PlainMessage(einfo));
                }
                File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "\\log_error.txt", einfo);
            }
        }
        public static void OnFriendRequest(IApplyResponseArgs args)
        {
            if (BotReg.GetBool("CORE/ALLOW_FRIEND_REQUEST", false))
            {
                BotAPI.HandleNewFriendApplyAsync(args, FriendApplyAction.Allow).Wait();
                if (BotAuth.HasOwner && BotReg.GetBool("LOG/FRIEND_REQUEST", true))
                {
                    _ = BotAPI.SendPrivateMessageAsync(BotAuth.Owner, new PlainMessage($"Accept friend request: {args.FromQQ}"));
                }
                _ = BotInfo.ReloadFriends();
            }
        }
    }
}