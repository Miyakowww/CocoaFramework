using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoaFramework.Support
{
    public static class BotInfo
    {
        private static readonly Dictionary<long, HashSet<long>> groupMembers = new();
        private static readonly HashSet<long> friends = new();

        public static async Task ReloadAll()
        {
            Task f = ReloadFriends();
            Task g = ReloadAllGroupMembers();

            await f;
            await g;
        }

        public static async Task ReloadAllGroupMembers()
        {
            groupMembers.Clear();
            foreach (var info in await BotAPI.GetGroupListAsync())
            {
                groupMembers.Add(info.Id, new((await BotAPI.GetGroupMemberListAsync(info.Id)).Select(i => i.Id)));
            }
        }
        public static async Task<bool> ReloadGroupMembers(long groupID)
        {
            if (groupMembers.ContainsKey(groupID))
            {
                groupMembers[groupID] = new((await BotAPI.GetGroupMemberListAsync(groupID)).Select(i => i.Id));
                return true;
            }
            else if ((await BotAPI.GetGroupListAsync()).Where(i => i.Id == groupID).Any())
            {
                groupMembers.Add(groupID, new((await BotAPI.GetGroupMemberListAsync(groupID)).Select(i => i.Id)));
                return true;
            }
            return false;
        }

        public static async Task ReloadFriends()
        {
            friends.Clear();
            foreach (var info in await BotAPI.GetFriendListAsync())
            {
                friends.Add(info.Id);
            }
        }

        public static bool HasGroup(long groupID)
            => groupMembers.ContainsKey(groupID);

        public static bool HasGroupMember(long groupID, long memberID)
            => groupMembers.ContainsKey(groupID) && groupMembers[groupID].Contains(memberID);

        public static bool HasFriend(long qqID)
            => friends.Contains(qqID);

        public static long[] GetTempPath(long qqID)
            => groupMembers
            .Where(p => p.Value.Contains(qqID))
            .Select(p => p.Key)
            .ToArray();
    }
}
