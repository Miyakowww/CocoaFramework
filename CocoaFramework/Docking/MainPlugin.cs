// Copyright 2020-2021 Miyakowww.
//
// 此源代码的使用受 GNU AFFERO GENERAL PUBLIC LICENSE version 3 许可证的约束, 可以在以下链接找到该许可证.
// Use of this source code is governed by the GNU AGPLv3 license that can be found through the following link.
//
// https://github.com/Miyakowww/CocoaFramework/blob/main/LICENSE

using System.Threading.Tasks;
using CocoaFramework.Core;
using CocoaFramework.Model;
using CocoaFramework.Support;
using Mirai_CSharp;
using Mirai_CSharp.Models;
using Mirai_CSharp.Plugin.Interfaces;

namespace CocoaFramework.Docking
{
    // Message Event
    internal partial class MainPlugin :
        IGroupMessage,
        IFriendMessage,
        ITempMessage
    {
        public Task<bool> FriendMessage(MiraiHttpSession session, IFriendMessageEventArgs e)
        {
            BotCore.OnMessage(new MessageSource(e.Sender.Id), new QMessage(e.Chain));
            return Task.FromResult(true);
        }

        public Task<bool> GroupMessage(MiraiHttpSession session, IGroupMessageEventArgs e)
        {
            BotCore.OnMessage(new MessageSource(e.Sender.Group.Id, e.Sender.Id, false), new QMessage(e.Chain));
            return Task.FromResult(true);
        }

        public Task<bool> TempMessage(MiraiHttpSession session, ITempMessageEventArgs e)
        {
            BotCore.OnMessage(new MessageSource(e.Sender.Group.Id, e.Sender.Id, true), new QMessage(e.Chain));
            return Task.FromResult(true);
        }
    }

    // Bot Event
    internal partial class MainPlugin :
        IBotRelogin
    {
        public async Task<bool> BotRelogin(MiraiHttpSession session, IBotReloginEventArgs e)
        {
            await BotInfo.ReloadAll();
            return true;
        }
    }

    // Friend Event
    internal partial class MainPlugin :
        INewFriendApply
    {
        public Task<bool> NewFriendApply(MiraiHttpSession session, INewFriendApplyEventArgs e)
        {
            BotCore.OnFriendRequest(e);
            return Task.FromResult(true);
        }
    }

    // Group Event
    internal partial class MainPlugin :
        IBotInvitedJoinGroup,
        IBotJoinedGroup,
        IBotPositiveLeaveGroup,
        IBotKickedOut,
        IGroupMemberJoined,
        IGroupMemberKicked,
        IGroupMemberPositiveLeave
    {
        public async Task<bool> BotInvitedJoinGroup(MiraiHttpSession session, IBotInvitedJoinGroupEventArgs e)
        {
            await BotInfo.ReloadGroupMembers(e.FromGroup);
            return true;
        }

        public async Task<bool> BotJoinedGroup(MiraiHttpSession session, IBotJoinedGroupEventArgs e)
        {
            await BotInfo.ReloadGroupMembers(e.Group.Id);
            return true;
        }

        public async Task<bool> BotKickedOut(MiraiHttpSession session, IBotKickedOutEventArgs e)
        {
            await BotInfo.ReloadAllGroupMembers();
            return true;
        }

        public async Task<bool> BotPositiveLeaveGroup(MiraiHttpSession session, IBotPositiveLeaveGroupEventArgs e)
        {
            await BotInfo.ReloadAllGroupMembers();
            return true;
        }

        public async Task<bool> GroupMemberJoined(MiraiHttpSession session, IGroupMemberJoinedEventArgs e)
        {
            await BotInfo.ReloadGroupMembers(e.Member.Group.Id);
            return true;
        }

        public async Task<bool> GroupMemberKicked(MiraiHttpSession session, IGroupMemberKickedEventArgs e)
        {
            await BotInfo.ReloadGroupMembers(e.Member.Group.Id);
            return true;
        }

        public async Task<bool> GroupMemberPositiveLeave(MiraiHttpSession session, IGroupMemberPositiveLeaveEventArgs e)
        {
            await BotInfo.ReloadGroupMembers(e.Member.Group.Id);
            return true;
        }
    }
}
