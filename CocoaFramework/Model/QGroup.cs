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
        public readonly long ID;

        public QGroup(long id)
        {
            ID = id;
        }

        public override bool Equals(object? obj) => obj is QGroup group && group.ID == ID;
        public override int GetHashCode() => ID.GetHashCode();

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
            return BotAPI.SendGroupMessageAsync(ID, new PlainMessage(message));
        }
        public Task<int> SendMessageAsync(params IMessageBase[] chain)
        {
            return BotAPI.SendGroupMessageAsync(ID, chain);
        }
    }
}
