// Copyright 2020-2021 Miyakowww.
//
// 此源代码的使用受 GNU AFFERO GENERAL PUBLIC LICENSE version 3 许可证的约束, 可以在以下链接找到该许可证.
// Use of this source code is governed by the GNU AGPLv3 license that can be found through the following link.
//
// https://github.com/Miyakowww/CocoaFramework/blob/main/LICENSE

using System;
using System.Threading.Tasks;
using CocoaFramework.Support;
using Mirai_CSharp.Models;

namespace CocoaFramework.Model
{
    public class QGroup
    {
        public long ID { get; }

        public QGroup(long id)
        {
            ID = id;
        }

        public override bool Equals(object? obj)
            => obj is QGroup group && group.ID == ID;
        public override int GetHashCode()
            => ID.GetHashCode();

        public int SendMessage(string message)
            => SendMessageAsync(message).Result;

        public int SendMessage(params IMessageBase[] chain)
            => SendMessageAsync(chain).Result;

        public Task<int> SendMessageAsync(string message)
            => BotAPI.SendGroupMessageAsync(ID, new PlainMessage(message));

        public Task<int> SendMessageAsync(params IMessageBase[] chain)
            => BotAPI.SendGroupMessageAsync(ID, chain);

        public int SendImage(string path)
            => SendImageAsync(path).Result;

        public async Task<int> SendImageAsync(string path)
            => await BotAPI.SendGroupMessageAsync(ID, await BotAPI.UploadImageAsync(UploadTarget.Group, path));

        public int SendVoice(string path)
            => SendVoiceAsync(path).Result;

        public async Task<int> SendVoiceAsync(string path)
            => await BotAPI.SendGroupMessageAsync(ID, await BotAPI.UploadVoiceAsync(UploadTarget.Group, path));

        public IGroupMemberCardInfo GetMemberCard(long qqID)
            => GetMemberCardAsync(qqID).Result;

        public Task<IGroupMemberCardInfo> GetMemberCardAsync(long qqID)
            => BotAPI.GetGroupMemberCardAsync(ID, qqID);

        public void MuteMember(long qqID, TimeSpan duration)
            => MuteMemberAsync(qqID, duration);

        public Task MuteMemberAsync(long qqID, TimeSpan duration)
            => BotAPI.MuteGroupMemberAsync(ID, qqID, duration);

        public void UnmuteMember(long qqID)
            => UnmuteMemberAsync(qqID);

        public Task UnmuteMemberAsync(long qqID)
            => BotAPI.UnmuteGroupMemberAsync(ID, qqID);

        public void MuteGroup()
            => MuteGroupAsync();

        public Task MuteGroupAsync()
            => BotAPI.MuteGroupAsync(ID);

        public void UnmuteGroup()
            => UnmuteGroupAsync();

        public Task UnmuteGroupAsync()
            => BotAPI.UnmuteGroupAsync(ID);

        public void KickMember(long qqID)
            => KickMemberAsync(qqID);

        public Task KickMemberAsync(long qqID)
            => BotAPI.KickGroupMemberAsync(ID, qqID);

        public void LeaveGroup()
            => LeaveGroupAsync();

        public Task LeaveGroupAsync()
            => BotAPI.LeaveGroupAsync(ID);
    }
}
