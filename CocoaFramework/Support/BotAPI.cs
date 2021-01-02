using Mirai_CSharp;
using Mirai_CSharp.Models;
using System;
using System.Collections.Generic;
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

        public static Task<int> SendFriendMessageAsync(long qid, string message)
        {
            return session!.SendFriendMessageAsync(qid, new PlainMessage(message));
        }
        public static Task<int> SendFriendMessageAsync(long qid, params IMessageBase[] chain)
        {
            return session!.SendFriendMessageAsync(qid, chain);
        }
        public static Task<int> SendFriendMessageAsync(int quote, long qid, string message)
        {
            return session!.SendFriendMessageAsync(qid, new IMessageBase[] { new PlainMessage(message) }, quote);
        }
        public static Task<int> SendFriendMessageAsync(int quote, long qid, params IMessageBase[] chain)
        {
            return session!.SendFriendMessageAsync(qid, chain, quote);
        }

        public static Task<int> SendTempMessageAsync(long qid, long gid, string message)
        {
            return session!.SendTempMessageAsync(qid, gid, new PlainMessage(message));
        }
        public static Task<int> SendTempMessageAsync(long qid, long gid, params IMessageBase[] chain)
        {
            return session!.SendTempMessageAsync(qid, gid, chain);
        }
        public static Task<int> SendTempMessageAsync(int quote, long qid, long gid, string message)
        {
            return session!.SendTempMessageAsync(qid, gid, new IMessageBase[] { new PlainMessage(message) }, quote);
        }
        public static Task<int> SendTempMessageAsync(int quote, long qid, long gid, params IMessageBase[] chain)
        {
            return session!.SendTempMessageAsync(qid, gid, chain, quote);
        }

        public static Task<int> SendPrivateMessageAsync(long qid, string message)
        {
            return SendPrivateMessageAsync(qid, new PlainMessage(message));
        }
        public static async Task<int> SendPrivateMessageAsync(long qid, params IMessageBase[] chain)
        {
            if (BotInfo.HasFriend(qid))
            {
                return await session!.SendFriendMessageAsync(qid, chain);
            }
            int id = 0;
            long[] tmp = BotInfo.GetTempPath(qid);
            foreach (var t in tmp)
            {
                try
                {
                    id = await SendTempMessageAsync(qid, t, chain);
                    break;
                }
                catch { }
            }
            return id;
        }
        public static Task<int> SendPrivateMessageAsync(int quote, long qid, string message)
        {
            return SendPrivateMessageAsync(quote, qid, new PlainMessage(message));
        }
        public static async Task<int> SendPrivateMessageAsync(int quote, long qid, params IMessageBase[] chain)
        {

            if (BotInfo.HasFriend(qid))
            {
                return await session!.SendFriendMessageAsync(qid, chain, quote);
            }
            int id = 0;
            long[] tmp = BotInfo.GetTempPath(qid);
            foreach (var t in tmp)
            {
                try
                {
                    id = await SendTempMessageAsync(quote, qid, t, chain);
                    break;
                }
                catch { }
            }
            return id;
        }

        public static Task<int> SendGroupMessageAsync(long gid, string message)
        {
            return SendGroupMessageAsync(gid, new PlainMessage(message));
        }
        public static Task<int> SendGroupMessageAsync(long gid, params IMessageBase[] chain)
        {
            return session!.SendGroupMessageAsync(gid, chain);
        }
        public static Task<int> SendGroupMessageAsync(int quote, long gid, string message)
        {
            return SendGroupMessageAsync(quote, gid, new PlainMessage(message));
        }
        public static Task<int> SendGroupMessageAsync(int quote, long gid, params IMessageBase[] chain)
        {
            return session!.SendGroupMessageAsync(gid, chain, quote);
        }

        public static Task<IFriendInfo[]> GetFriendListAsync()
        {
            return session!.GetFriendListAsync();
        }

        public static Task<IGroupMemberInfo[]> GetGroupMemberListAsync(long gid)
        {
            return session!.GetGroupMemberListAsync(gid);
        }
        public static Task<IGroupMemberCardInfo> GetGroupMemberInfoAsync(long gid, long uid)
        {
            return session!.GetGroupMemberInfoAsync(uid, gid);
        }
        public static Task<IGroupInfo[]> GetGroupListAsync()
        {
            return session!.GetGroupListAsync();
        }

        public static Task<ImageMessage> UploadImageAsync(UploadTarget target, string path)
        {
            return session!.UploadPictureAsync(target, path);
        }

        public static Task<VoiceMessage> UploadVoiceAsync(UploadTarget target, string path)
        {
            return session!.UploadVoiceAsync(target, path);
        }

        public static Task KickMemberAsync(long gid, long uid)
        {
            return session!.KickMemberAsync(uid, gid);
        }

        public static Task LeaveGroupAsync(long gid)
        {
            return session!.LeaveGroupAsync(gid);
        }

        public static Task MuteAsync(long gid, long uid, TimeSpan duration)
        {
            return session!.MuteAsync(uid, gid, duration);
        }
        public static Task UnmuteAsync(long gid, long uid)
        {
            return session!.UnmuteAsync(uid, gid);
        }
        public static Task MuteAllAsync(long gid)
        {
            return session!.MuteAllAsync(gid);
        }
        public static Task UnmuteAllAsync(long gid)
        {
            return session!.UnmuteAllAsync(gid);
        }
    }
}
