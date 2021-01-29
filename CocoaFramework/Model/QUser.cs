// Copyright 2020-2021 Miyakowww.
//
// 此源代码的使用受 GNU AFFERO GENERAL PUBLIC LICENSE version 3 许可证的约束, 可以在以下链接找到该许可证.
// Use of this source code is governed by the GNU AGPLv3 license that can be found through the following link.
//
// https://github.com/Miyakowww/CocoaFramework/blob/main/LICENSE

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
        public long ID { get; }

        public QUser(long id)
        {
            ID = id;
        }

        public override bool Equals(object? obj)
            => obj is QUser user && user.ID == ID;
        public override int GetHashCode()
            => ID.GetHashCode();

        public bool IsFriend => BotInfo.HasFriend(ID);
        public bool IsOwner => BotAuth.IsOwner(ID);
        public bool IsAdmin => BotAuth.IsAdmin(ID);
        public int AuthLevel => BotAuth.AuthLevel(ID);

        public int SendMessage(string message)
            => SendMessageAsync(message).Result;

        public int SendMessage(params IMessageBase[] chain)
            => SendMessageAsync(chain).Result;

        public Task<int> SendMessageAsync(string message)
            => BotAPI.SendPrivateMessageAsync(ID, new PlainMessage(message));

        public Task<int> SendMessageAsync(params IMessageBase[] chain)
            => BotAPI.SendPrivateMessageAsync(ID, chain);

        public int SendImage(string path)
            => SendImageAsync(path).Result;

        public async Task<int> SendImageAsync(string path)
            => await BotAPI.SendPrivateMessageAsync(ID, await BotAPI.UploadImageAsync(IsFriend ? UploadTarget.Friend : UploadTarget.Temp, path));

        public int SendVoice(string path)
            => SendVoiceAsync(path).Result;

        public async Task<int> SendVoiceAsync(string path)
            => await BotAPI.SendPrivateMessageAsync(ID, await BotAPI.UploadVoiceAsync(IsFriend ? UploadTarget.Friend : UploadTarget.Temp, path));
    }
}
