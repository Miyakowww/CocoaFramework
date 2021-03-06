﻿// Copyright 2020-2021 Miyakowww.
//
// 此源代码的使用受 GNU AFFERO GENERAL PUBLIC LICENSE version 3 许可证的约束, 可以在以下链接找到该许可证.
// Use of this source code is governed by the GNU AGPLv3 license that can be found through the following link.
//
// https://github.com/Miyakowww/CocoaFramework/blob/main/LICENSE

using CocoaFramework.Model;

namespace CocoaFramework.Core.ProcessingModel
{
    public record ListeningTarget
    {
        public readonly QGroup? group;
        public readonly QUser? user;

        internal bool Fit(MessageSource src)
        {
            bool gFit = group is null || group.ID == src?.group?.ID;
            bool uFit = user is null || user.ID == src?.user?.ID;
            return gFit && uFit;
        }

        private ListeningTarget(QGroup? group, QUser? user)
        {
            this.group = group;
            this.user = user;
        }

        public static readonly ListeningTarget All = new(null, null);
        public static ListeningTarget FromGroup(long group) 
            => new ListeningTarget(new QGroup(group), null);
        public static ListeningTarget? FromGroup(QGroup group)
        {
            if (group is null)
            {
                return null;
            }
            return new ListeningTarget(group, null);
        }
        public static ListeningTarget FromUser(long user) 
            => new ListeningTarget(null, new QUser(user));
        public static ListeningTarget? FromUser(QUser user)
        {
            if (user is null)
            {
                return null;
            }
            return new ListeningTarget(null, user);
        }
        public static ListeningTarget FromTarget(long group, long user) 
            => new ListeningTarget(new QGroup(group), new QUser(user));
        public static ListeningTarget? FromTarget(MessageSource src)
        {
            if (src is null)
            {
                return null;
            }
            return new ListeningTarget(src?.group, src?.user);
        }
        public static ListeningTarget? FromTarget(QGroup group, QUser user)
        {
            if (group is null || user is null)
            {
                return null;
            }
            return new ListeningTarget(group, user);
        }
    }
}
