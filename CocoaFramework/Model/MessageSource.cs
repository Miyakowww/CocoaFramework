using CocoaFramework.Support;
using Mirai_CSharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoaFramework.Model
{
    public class MessageSource
    {
        public readonly QGroup? group;
        public readonly QUser user;

        public bool IsGroup { get; }
        public bool IsTemp { get; }

        public bool IsFriend => user.IsFriend;
        public bool IsOwner => user.IsOwner;
        public bool IsAdmin => user.IsAdmin;
        public int AuthLevel => user.AuthLevel;

        public MessageSource(long qqID)
        {
            IsGroup = false;
            IsTemp = false;
            group = null;
            user = new(qqID);
        }
        public MessageSource(long groupID, long qqID, bool isTemp)
        {
            IsGroup = !isTemp;
            IsTemp = isTemp;
            group = new(groupID);
            user = new(qqID);
        }

        public override bool Equals(object? obj)
            => obj is MessageSource src
            && src.IsGroup == IsGroup
            && src.IsTemp == IsTemp
            && src.group?.ID == group?.ID
            && src.user.ID == user.ID;
        public override int GetHashCode()
            => (IsGroup || IsTemp) ?
            group!.ID.GetHashCode() ^ user.ID.GetHashCode() :
            user.ID.GetHashCode();

        public int Send(string message)
            => SendAsync(new PlainMessage(message)).Result;
        public int Send(params IMessageBase[] chain)
            => SendAsync(chain).Result;

        public Task<int> SendAsync(string message)
        {
            if (IsGroup)
            {
                return BotAPI.SendGroupMessageAsync(group!.ID, new PlainMessage(message));
            }
            else if (IsTemp)
            {
                return BotAPI.SendTempMessageAsync(user.ID, group!.ID, new PlainMessage(message));
            }
            else
            {
                return BotAPI.SendFriendMessageAsync(user.ID, new PlainMessage(message));
            }
        }
        public Task<int> SendAsync(params IMessageBase[] chain)
        {
            if (IsGroup)
            {
                return BotAPI.SendGroupMessageAsync(group!.ID, chain);
            }
            else if (IsTemp)
            {
                return BotAPI.SendTempMessageAsync(user.ID, group!.ID, chain);
            }
            else
            {
                return BotAPI.SendFriendMessageAsync(user.ID, chain);
            }
        }

        public int SendEx(bool addAtWhenGroup, string groupDelimiter, string message)
            => SendExAsync(addAtWhenGroup, groupDelimiter, new PlainMessage(message)).Result;

        public int SendEx(bool addAtWhenGroup, string groupDelimiter, params IMessageBase[] chain)
            => SendExAsync(addAtWhenGroup, groupDelimiter, chain).Result;

        public Task<int> SendExAsync(bool addAtWhenGroup, string groupDelimiter, string message)
            => SendExAsync(addAtWhenGroup, groupDelimiter, new PlainMessage(message));

        public Task<int> SendExAsync(bool addAtWhenGroup, string groupDelimiter, params IMessageBase[] chain)
        {
            if (IsGroup)
            {
                List<IMessageBase> newChain = new(chain.Length + 2);
                if (addAtWhenGroup)
                {
                    newChain.Add(new AtMessage(user.ID));
                }
                newChain.Add(new PlainMessage(groupDelimiter));
                newChain.AddRange(chain);
                return BotAPI.SendGroupMessageAsync(group!.ID, newChain.ToArray());
            }
            else if (IsTemp)
            {
                return BotAPI.SendTempMessageAsync(user.ID, group!.ID, chain);
            }
            else
            {
                return BotAPI.SendFriendMessageAsync(user.ID, chain);
            }
        }


        public int SendReplyEx(QMessage quote, bool addAtWhenGroup, string message)
            => SendReplyExAsync(quote, addAtWhenGroup, new PlainMessage(message)).Result;

        public int SendReplyEx(QMessage quote, bool addAtWhenGroup, params IMessageBase[] chain)
            => SendReplyExAsync(quote, addAtWhenGroup, chain).Result;

        public Task<int> SendReplyExAsync(QMessage quote, bool addAtWhenGroup, string message)
            => SendReplyExAsync(quote, addAtWhenGroup, new PlainMessage(message));

        public Task<int> SendReplyExAsync(QMessage quote, bool addAtWhenGroup, params IMessageBase[] chain)
        {
            if (IsGroup)
            {
                List<IMessageBase> newChain = new(chain.Length + 2);
                if (addAtWhenGroup)
                {
                    newChain.Add(new AtMessage(user.ID));
                    newChain.Add(new PlainMessage(" "));
                }
                newChain.AddRange(chain);
                return BotAPI.SendGroupMessageAsync(quote.ID, group!.ID, newChain.ToArray());
            }
            else if (IsTemp)
            {
                return BotAPI.SendTempMessageAsync(quote.ID, user.ID, group!.ID, chain);
            }
            else
            {
                return BotAPI.SendFriendMessageAsync(quote.ID, user.ID, chain);
            }
        }


        public int SendPrivate(string message)
            => SendPrivateAsync(new PlainMessage(message)).Result;

        public int SendPrivate(params IMessageBase[] chain)
            => SendPrivateAsync(chain).Result;

        public Task<int> SendPrivateAsync(string message)
            => SendPrivateAsync(new PlainMessage(message));

        public Task<int> SendPrivateAsync(params IMessageBase[] chain)
        {
            if (IsTemp || !user.IsFriend)
            {
                try
                {
                    return BotAPI.SendTempMessageAsync(user.ID, group!.ID, chain);
                }
                catch
                {
                    return BotAPI.SendPrivateMessageAsync(user.ID, chain);
                }
            }
            else
            {
                return BotAPI.SendFriendMessageAsync(user.ID, chain);
            }
        }

        public int SendImage(string path)
            => SendImageAsync(path).Result;

        public async Task<int> SendImageAsync(string path)
            => await SendAsync(await BotAPI.UploadImageAsync(IsGroup ? UploadTarget.Group : (IsFriend ? UploadTarget.Friend : UploadTarget.Temp), path));

        public int SendVoice(string path)
            => SendVoiceAsync(path).Result;

        public async Task<int> SendVoiceAsync(string path)
            => await SendAsync(await BotAPI.UploadVoiceAsync(IsGroup ? UploadTarget.Group : (IsFriend ? UploadTarget.Friend : UploadTarget.Temp), path));

        public void Mute(TimeSpan duration)
            => MuteAsync(duration);

        public Task MuteAsync(TimeSpan duration)
            => IsGroup ? BotAPI.MuteGroupMemberAsync(group!.ID, user.ID, duration) : Task.CompletedTask;

        public void Unmute()
            => UnmuteAsync();

        public Task UnmuteAsync()
            => IsGroup ? BotAPI.UnmuteGroupMemberAsync(group!.ID, user.ID) : Task.CompletedTask;
    }
}
