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

        public void Mute(long uid, TimeSpan duration)
            => MuteAsync(uid, duration);

        public Task MuteAsync(long uid, TimeSpan duration)
            => BotAPI.MuteAsync(ID, uid, duration);

        public void Unmute(long uid)
            => UnmuteAsync(uid);

        public Task UnmuteAsync(long uid)
            => BotAPI.UnmuteAsync(ID, uid);

        public void MuteAll()
            => MuteAllAsync();

        public Task MuteAllAsync()
            => BotAPI.MuteAllAsync(ID);

        public void UnmuteAll()
            => UnmuteAllAsync();

        public Task UnmuteAllAsync()
            => BotAPI.UnmuteAllAsync(ID);

        public void Kick(long uid)
            => KickAsync(uid);

        public Task KickAsync(long uid)
            => BotAPI.KickMemberAsync(ID, uid);

        public void Leave()
            => LeaveAsync();

        public Task LeaveAsync()
            => BotAPI.LeaveGroupAsync(ID);
    }
}
