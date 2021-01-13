using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CocoaFramework.Support
{
    public static class BotInfo
    {
        private static Dictionary<long, HashSet<long>>? groupMembers;
        private static ImmutableHashSet<long>? friends;

        public static async Task ReloadAll()
        {
            Task f = ReloadFriends();
            Task g = ReloadAllGroupMembers();

            await f;
            await g;
        }

        public static async Task ReloadAllGroupMembers()
        {
            Dictionary<long, HashSet<long>> _groupMembers = new();
            foreach (var info in await BotAPI.GetGroupListAsync())
            {
                _groupMembers.Add(info.Id, new((await BotAPI.GetGroupMemberListAsync(info.Id)).Select(i => i.Id)));
            }
            groupMembers = _groupMembers;
        }
        public static async Task<bool> ReloadGroupMembers(long groupID)
        {
            if (groupMembers is null)
            {
                return false;
            }
            if ((await BotAPI.GetGroupListAsync()).Where(i => i.Id == groupID).Any())
            {
                groupMembers[groupID] = new((await BotAPI.GetGroupMemberListAsync(groupID)).Select(i => i.Id));
                return true;
            }
            return false;
        }

        public static async Task ReloadFriends()
        {
            friends = (await BotAPI.GetFriendListAsync()).Select(i => i.Id).ToImmutableHashSet();
        }

        public static bool HasGroup(long groupID)
            => groupMembers?.ContainsKey(groupID) ?? false;

        public static bool HasGroupMember(long groupID, long memberID)
            => (groupMembers?.ContainsKey(groupID) ?? false) && groupMembers![groupID].Contains(memberID);

        public static bool HasFriend(long qqID)
            => friends?.Contains(qqID) ?? false;

        public static long[] GetTempPath(long qqID)
            => groupMembers?
            .Where(p => p.Value.Contains(qqID))
            .Select(p => p.Key)
            .ToArray()
            is long[] retval ? retval : Array.Empty<long>();
    }
}
