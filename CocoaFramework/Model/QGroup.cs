using CocoaFramework.Support;
using Mirai_CSharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
