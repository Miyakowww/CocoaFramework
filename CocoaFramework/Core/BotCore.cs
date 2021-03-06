﻿// Copyright 2020-2021 Miyakowww.
//
// 此源代码的使用受 GNU AFFERO GENERAL PUBLIC LICENSE version 3 许可证的约束, 可以在以下链接找到该许可证.
// Use of this source code is governed by the GNU AGPLv3 license that can be found through the following link.
//
// https://github.com/Miyakowww/CocoaFramework/blob/main/LICENSE

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CocoaFramework.Docking;
using CocoaFramework.Model;
using CocoaFramework.Support;
using Mirai_CSharp;
using Mirai_CSharp.Models;

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
                StringBuilder sb = new(DateTime.Now.ToString());

                sb.Append($"\nMessage: ");
                if (src.IsGroup)
                {
                    sb.Append($"[{src.group!.ID}] ");
                }
                sb.Append($"[{src.user.ID}] ");
                sb.Append(msg);

                sb.Append($"\n{e}");

                if (BotReg.GetBool("CORE/LOG_ERROR", true) && BotAuth.HasOwner)
                {
                    _ = BotAPI.SendPrivateMessageAsync(BotAuth.Owner, new PlainMessage(sb.ToString()));
                }
                sb.Append("\n\n");
                File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "/log_error.txt", sb.ToString());
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