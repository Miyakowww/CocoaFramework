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
using System.Threading.Tasks;

#pragma warning disable CS0618 // 用户专用的警告

namespace CocoaFramework.Core
{
    internal static class BotCore
    {
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
        }

        public static void Message(MessageSource source, QMessage msg)
        {
            try
            {
                QMessage newMsg = msg;

                bool release = MiddlewareCore.Run(ref source, ref newMsg);
                if (release)
                {
                    int stat = ModuleCore.Run(source, newMsg);
                    ServiceCore.Run(source, newMsg, msg, stat != -1, stat >= 0 ? ModuleCore.Modules[stat] : null);
                }
            }
            catch (Exception e)
            {
                string einfo = $"\n{DateTime.Now}\nMessage:{(source.IsGroup ? $"[{source.group!.ID}]" : "")}[{source.user.ID}] {msg.PlainText}\n{e.StackTrace}\n{e.Message}";
                if (BotReg.GetBool("CORE/LOG_ERROR", true) && BotAuth.HasOwner)
                {
                    _ = BotAPI.SendPrivateMessageAsync(BotAuth.Owner, new PlainMessage(einfo));
                }
                File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "\\log_error.txt", einfo);
            }
        }
        public static void FriendRequest(IApplyResponseArgs args)
        {
            if (BotReg.GetBool("CORE/ALLOW_FRIEND_REQUEST", true))
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