using Mirai_CSharp;
using Mirai_CSharp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoaFramework.Support
{
    public static class BotAPI
    {
        private static MiraiHttpSession? session;

        internal static void Init(MiraiHttpSession _session)
        {
            session = _session;
        }

        public static long BotQQ => session!.QQNumber.GetValueOrDefault();

        public static Task HandleNewFriendApplyAsync(IApplyResponseArgs args, FriendApplyAction action)
        {
            return session!.HandleNewFriendApplyAsync(args, action);
        }

        public static Task<int> SendFriendMessageAsync(long qqID, string message)
        {
            return session!.SendFriendMessageAsync(qqID, new PlainMessage(message));
        }
        public static Task<int> SendFriendMessageAsync(long qqID, params IMessageBase[] chain)
        {
            return session!.SendFriendMessageAsync(qqID, chain);
        }
        public static Task<int> SendFriendMessageAsync(int quote, long qqID, string message)
        {
            return session!.SendFriendMessageAsync(qqID, new IMessageBase[] { new PlainMessage(message) }, quote);
        }
        public static Task<int> SendFriendMessageAsync(int quote, long qqID, params IMessageBase[] chain)
        {
            return session!.SendFriendMessageAsync(qqID, chain, quote);
        }

        public static Task<int> SendTempMessageAsync(long qqID, long groupID, string message)
        {
            return session!.SendTempMessageAsync(qqID, groupID, new PlainMessage(message));
        }
        public static Task<int> SendTempMessageAsync(long qqID, long groupID, params IMessageBase[] chain)
        {
            return session!.SendTempMessageAsync(qqID, groupID, chain);
        }
        public static Task<int> SendTempMessageAsync(int quote, long qqID, long groupID, string message)
        {
            return session!.SendTempMessageAsync(qqID, groupID, new IMessageBase[] { new PlainMessage(message) }, quote);
        }
        public static Task<int> SendTempMessageAsync(int quote, long qqID, long groupID, params IMessageBase[] chain)
        {
            return session!.SendTempMessageAsync(qqID, groupID, chain, quote);
        }

        public static Task<int> SendPrivateMessageAsync(long qqID, string message)
        {
            return SendPrivateMessageAsync(qqID, new PlainMessage(message));
        }
        public static async Task<int> SendPrivateMessageAsync(long qqID, params IMessageBase[] chain)
        {
            if (BotInfo.HasFriend(qqID))
            {
                return await session!.SendFriendMessageAsync(qqID, chain);
            }
            int id = 0;
            long[] tempPath = BotInfo.GetTempPath(qqID);
            foreach (var t in tempPath)
            {
                try
                {
                    id = await SendTempMessageAsync(qqID, t, chain);
                    break;
                }
                catch { }
            }
            return id;
        }
        public static Task<int> SendPrivateMessageAsync(int quote, long qqID, string message)
        {
            return SendPrivateMessageAsync(quote, qqID, new PlainMessage(message));
        }
        public static async Task<int> SendPrivateMessageAsync(int quote, long qqID, params IMessageBase[] chain)
        {

            if (BotInfo.HasFriend(qqID))
            {
                return await session!.SendFriendMessageAsync(qqID, chain, quote);
            }
            int id = 0;
            long[] tempPath = BotInfo.GetTempPath(qqID);
            foreach (var t in tempPath)
            {
                try
                {
                    id = await SendTempMessageAsync(quote, qqID, t, chain);
                    break;
                }
                catch { }
            }
            return id;
        }

        public static Task<int> SendGroupMessageAsync(long groupID, string message)
        {
            return SendGroupMessageAsync(groupID, new PlainMessage(message));
        }
        public static Task<int> SendGroupMessageAsync(long groupID, params IMessageBase[] chain)
        {
            return session!.SendGroupMessageAsync(groupID, chain);
        }
        public static Task<int> SendGroupMessageAsync(int quote, long groupID, string message)
        {
            return SendGroupMessageAsync(quote, groupID, new PlainMessage(message));
        }
        public static Task<int> SendGroupMessageAsync(int quote, long groupID, params IMessageBase[] chain)
        {
            return session!.SendGroupMessageAsync(groupID, chain, quote);
        }

        public static Task<IFriendInfo[]> GetFriendListAsync()
        {
            return session!.GetFriendListAsync();
        }

        public static Task<IGroupMemberInfo[]> GetGroupMemberListAsync(long groupID)
        {
            return session!.GetGroupMemberListAsync(groupID);
        }
        public static Task<IGroupMemberCardInfo> GetGroupMemberCardAsync(long groupID, long qqID)
        {
            return session!.GetGroupMemberInfoAsync(qqID, groupID);
        }
        public static Task<IGroupInfo[]> GetGroupListAsync()
        {
            return session!.GetGroupListAsync();
        }

        public static async Task<ImageMessage> UploadImageAsync(UploadTarget target, string path)
        {
            using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return await session!.UploadPictureAsync(target, fs);
        }
        public static async Task<VoiceMessage> UploadVoiceAsync(UploadTarget target, string path)
        {
            using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return await session!.UploadVoiceAsync(target, fs);
        }

        public static Task KickGroupMemberAsync(long groupID, long qqID)
        {
            return session!.KickMemberAsync(qqID, groupID);
        }
        public static Task LeaveGroupAsync(long groupID)
        {
            return session!.LeaveGroupAsync(groupID);
        }

        public static Task MuteGroupMemberAsync(long groupID, long qqID, TimeSpan duration)
        {
            return session!.MuteAsync(qqID, groupID, duration);
        }
        public static Task UnmuteGroupMemberAsync(long groupID, long qqID)
        {
            return session!.UnmuteAsync(qqID, groupID);
        }
        public static Task MuteGroupAsync(long groupID)
        {
            return session!.MuteAllAsync(groupID);
        }
        public static Task UnmuteGroupAsync(long groupID)
        {
            return session!.UnmuteAllAsync(groupID);
        }
    }
}
