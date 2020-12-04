using CocoaFramework.Support;
using Mirai_CSharp.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CocoaFramework.Model
{
    public class QUser
    {
        public readonly long ID;

        public QUser(long id)
        {
            ID = id;
        }

        public override bool Equals(object? obj) => obj is QUser user && user.ID == ID;
        public override int GetHashCode() => ID.GetHashCode();

        public bool IsFriend => BotInfo.HasFriend(ID);
        public bool IsOwner => BotAuth.IsOwner(ID);
        public bool IsAdmin => BotAuth.IsAdmin(ID);
        public int AuthLevel => BotAuth.AuthLevel(ID);

        public int SendMessage(string message)
        {
            return SendMessageAsync(message).Result;
        }
        public int SendMessage(params IMessageBase[] chain)
        {
            return SendMessageAsync(chain).Result;
        }
        public Task<int> SendMessageAsync(string message)
        {
            return BotAPI.SendPrivateMessageAsync(ID, new PlainMessage(message));
        }
        public Task<int> SendMessageAsync(params IMessageBase[] chain)
        {
            return BotAPI.SendPrivateMessageAsync(ID, chain);
        }
    }
}
