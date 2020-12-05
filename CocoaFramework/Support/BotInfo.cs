using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoaFramework.Support
{
    public static class BotInfo
    {
        private static readonly Dictionary<long, HashSet<long>> relationship = new();
        private static readonly HashSet<long> friends = new();

        public static async Task ReloadAll()
        {
            Task f = ReloadFriends();
            Task g = ReloadGroups();

            await f;
            await g;
        }

        public static async Task ReloadGroups()
        {
            relationship.Clear();
            foreach (var info in await BotAPI.GetGroupListAsync())
            {
                relationship.Add(info.Id, new((await BotAPI.GetGroupMemberListAsync(info.Id)).Select(i => i.Id)));
            }
        }
        public static async Task<bool> ReloadGroup(long id)
        {
            if (relationship.ContainsKey(id))
            {
                relationship[id] = new((await BotAPI.GetGroupMemberListAsync(id)).Select(i => i.Id));
                return true;
            }
            else if ((await BotAPI.GetGroupListAsync()).Where(i => i.Id == id).Any())
            {
                relationship.Add(id, new((await BotAPI.GetGroupMemberListAsync(id)).Select(i => i.Id)));
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

        public static bool HasGroup(long id)
            => relationship.ContainsKey(id);

        public static bool HasGroupMember(long groupID, long memberID)
            => relationship.ContainsKey(groupID) && relationship[groupID].Contains(memberID);

        public static bool HasFriend(long id)
            => friends.Contains(id);

        public static long[] GetTempPath(long id)
            => relationship
            .Where(p => p.Value.Contains(id))
            .Select(p => p.Key)
            .ToArray();
    }
}
