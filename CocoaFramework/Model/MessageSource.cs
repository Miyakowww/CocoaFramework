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

        public readonly bool IsGroup;
        public readonly bool IsTemp;

        public bool IsFriend => user.IsFriend;
        public bool IsOwner => user.IsOwner;
        public bool IsAdmin => user.IsAdmin;
        public int AuthLevel => user.AuthLevel;

        public MessageSource(long qid)
        {
            IsGroup = false;
            IsTemp = false;
            group = null;
            user = new(qid);
        }
        public MessageSource(long gid, long qid, bool isTemp)
        {
            IsGroup = !isTemp;
            IsTemp = isTemp;
            group = new(gid);
            user = new(qid);
        }

        public override bool Equals(object? obj) => obj is MessageSource src
            && src.IsGroup == IsGroup
            && src.IsTemp == IsTemp
            && src.group?.ID == group?.ID
            && src.user.ID == user.ID;
        public override int GetHashCode() => (IsGroup || IsTemp) ?
            group!.ID.GetHashCode() ^ user.ID.GetHashCode() :
            user.ID.GetHashCode();

        public int Send(string message)
        {
            return SendAsync(new PlainMessage(message)).Result;
        }
        public int Send(params IMessageBase[] chain)
        {
            return SendAsync(chain).Result;
        }
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
        {
            return SendExAsync(addAtWhenGroup, groupDelimiter, new PlainMessage(message)).Result;
        }
        public int SendEx(bool addAtWhenGroup, string groupDelimiter, params IMessageBase[] chain)
        {
            return SendExAsync(addAtWhenGroup, groupDelimiter, chain).Result;
        }
        public Task<int> SendExAsync(bool addAtWhenGroup, string groupDelimiter, string message)
        {
            return SendExAsync(addAtWhenGroup, groupDelimiter, new PlainMessage(message));
        }
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
        {
            return SendReplyExAsync(quote, addAtWhenGroup, new PlainMessage(message)).Result;
        }
        public int SendReplyEx(QMessage quote, bool addAtWhenGroup, params IMessageBase[] chain)
        {
            return SendReplyExAsync(quote, addAtWhenGroup, chain).Result;
        }
        public Task<int> SendReplyExAsync(QMessage quote, bool addAtWhenGroup, string message)
        {
            return SendReplyExAsync(quote, addAtWhenGroup, new PlainMessage(message));
        }
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
        {
            return SendPrivateAsync(new PlainMessage(message)).Result;
        }
        public int SendPrivate(params IMessageBase[] chain)
        {
            return SendPrivateAsync(chain).Result;
        }
        public Task<int> SendPrivateAsync(string message)
        {
            return SendPrivateAsync(new PlainMessage(message));
        }
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
    }
}
