using CocoaFramework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        {
            return new ListeningTarget(new QGroup(group), null);
        }
        public static ListeningTarget? FromGroup(QGroup group)
        {
            if (group is null)
            {
                return null;
            }
            return new ListeningTarget(group, null);
        }
        public static ListeningTarget FromUser(long user)
        {
            return new ListeningTarget(null, new QUser(user));
        }
        public static ListeningTarget? FromUser(QUser user)
        {
            if (user is null)
            {
                return null;
            }
            return new ListeningTarget(null, user);
        }
        public static ListeningTarget FromTarget(long group, long user)
        {
            return new ListeningTarget(new QGroup(group), new QUser(user));
        }
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
