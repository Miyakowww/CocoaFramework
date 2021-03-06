﻿// Copyright 2020-2021 Miyakowww.
//
// 此源代码的使用受 GNU AFFERO GENERAL PUBLIC LICENSE version 3 许可证的约束, 可以在以下链接找到该许可证.
// Use of this source code is governed by the GNU AGPLv3 license that can be found through the following link.
//
// https://github.com/Miyakowww/CocoaFramework/blob/main/LICENSE

using System;
using System.IO;
using System.Threading.Tasks;
using CocoaFramework.Core;
using Mirai_CSharp;
using Mirai_CSharp.Models;

namespace CocoaFramework.Support
{
    public static class BotAPI
    {
        private static MiraiHttpSession session = null!;

        internal static void Init(MiraiHttpSession _session) => session = _session;

        public static long BotQQ => session.QQNumber ?? 0;

        public static Task HandleNewFriendApplyAsync(IApplyResponseArgs args, FriendApplyAction action)
            => session.HandleNewFriendApplyAsync(args, action);

        internal static async Task<int> CommonSendMessage(long id, bool isGroup, IMessageBase[] chain, int? quote)
        {
            if (!MiddlewareCore.OnSend(ref id, ref isGroup, ref chain, ref quote))
            {
                return 0;
            }

            if (isGroup)
            {
                return await session.SendGroupMessageAsync(id, chain, quote);
            }
            else
            {
                if (BotInfo.HasFriend(id))
                {
                    return await session.SendFriendMessageAsync(id, chain, quote);
                }
                int msgid = 0;
                long[] tempPath = BotInfo.GetTempPath(id);
                foreach (var t in tempPath)
                {
                    try
                    {
                        msgid = await session.SendTempMessageAsync(id, t, chain, quote);
                        break;
                    }
                    catch { }
                }
                return msgid;
            }
        }

        public static Task<int> SendPrivateMessageAsync(long qqID, string message)
            => SendPrivateMessageAsync(qqID, new PlainMessage(message));
        public static Task<int> SendPrivateMessageAsync(long qqID, params IMessageBase[] chain)
            => CommonSendMessage(qqID, false, chain, null);
        public static Task<int> SendPrivateMessageAsync(int quote, long qqID, string message)
            => SendPrivateMessageAsync(quote, qqID, new PlainMessage(message));
        public static Task<int> SendPrivateMessageAsync(int quote, long qqID, params IMessageBase[] chain)
            => CommonSendMessage(qqID, false, chain, quote);

        public static Task<int> SendGroupMessageAsync(long groupID, string message)
            => SendGroupMessageAsync(groupID, new PlainMessage(message));
        public static Task<int> SendGroupMessageAsync(long groupID, params IMessageBase[] chain)
            => CommonSendMessage(groupID, true, chain, null);
        public static Task<int> SendGroupMessageAsync(int quote, long groupID, string message)
            => SendGroupMessageAsync(quote, groupID, new PlainMessage(message));
        public static Task<int> SendGroupMessageAsync(int quote, long groupID, params IMessageBase[] chain)
            => CommonSendMessage(groupID, true, chain, quote);

        public static Task RevokeMessageAsync(int messageID)
            => session.RevokeMessageAsync(messageID);

        public static Task<IFriendInfo[]> GetFriendListAsync()
            => session.GetFriendListAsync();

        public static Task<IGroupMemberInfo[]> GetGroupMemberListAsync(long groupID)
            => session.GetGroupMemberListAsync(groupID);
        public static Task<IGroupMemberCardInfo> GetGroupMemberCardAsync(long groupID, long qqID)
            => session.GetGroupMemberInfoAsync(qqID, groupID);
        public static Task<IGroupInfo[]> GetGroupListAsync()
            => session.GetGroupListAsync();

        public static async Task<ImageMessage> UploadImageAsync(UploadTarget target, string path)
        { // Mirai-CSharp 的根据路径上传忘了释放创建的流
            using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return await session.UploadPictureAsync(target, fs); // 直接返回 Task 会导致流提前释放
        }
        public static async Task<VoiceMessage> UploadVoiceAsync(UploadTarget target, string path)
        { // Mirai-CSharp 的根据路径上传忘了释放创建的流
            using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return await session.UploadVoiceAsync(target, fs); // 直接返回 Task 会导致流提前释放
        }

        public static Task KickGroupMemberAsync(long groupID, long qqID)
            => session.KickMemberAsync(qqID, groupID);
        public static Task LeaveGroupAsync(long groupID)
            => session.LeaveGroupAsync(groupID);

        public static Task MuteGroupMemberAsync(long groupID, long qqID, TimeSpan duration)
            => session.MuteAsync(qqID, groupID, duration);
        public static Task UnmuteGroupMemberAsync(long groupID, long qqID)
            => session.UnmuteAsync(qqID, groupID);
        public static Task MuteGroupAsync(long groupID)
            => session.MuteAllAsync(groupID);
        public static Task UnmuteGroupAsync(long groupID)
            => session.UnmuteAllAsync(groupID);
    }
}
