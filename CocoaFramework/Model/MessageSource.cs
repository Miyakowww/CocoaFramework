﻿// Copyright 2020-2021 Miyakowww.
//
// 此源代码的使用受 GNU AFFERO GENERAL PUBLIC LICENSE version 3 许可证的约束, 可以在以下链接找到该许可证.
// Use of this source code is governed by the GNU AGPLv3 license that can be found through the following link.
//
// https://github.com/Miyakowww/CocoaFramework/blob/main/LICENSE

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CocoaFramework.Support;
using Mirai_CSharp.Models;

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
            => SendAsync(message).Result;

        public int Send(params IMessageBase[] chain)
            => SendAsync(chain).Result;

        public Task<int> SendAsync(string message)
            => SendAsync(new PlainMessage(message));

        public Task<int> SendAsync(params IMessageBase[] chain)
        {
            if (IsGroup)
            {
                return BotAPI.SendGroupMessageAsync(group!.ID, chain);
            }
            else
            {
                return BotAPI.SendPrivateMessageAsync(user.ID, chain);
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
            else
            {
                return BotAPI.SendPrivateMessageAsync(user.ID, chain);
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
            else
            {
                return BotAPI.SendPrivateMessageAsync(quote.ID, user.ID, chain);
            }
        }


        public int SendPrivate(string message)
            => SendPrivateAsync(new PlainMessage(message)).Result;

        public int SendPrivate(params IMessageBase[] chain)
            => SendPrivateAsync(chain).Result;

        public Task<int> SendPrivateAsync(string message)
            => SendPrivateAsync(new PlainMessage(message));

        public Task<int> SendPrivateAsync(params IMessageBase[] chain)
            => BotAPI.SendPrivateMessageAsync(user.ID, chain);


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
