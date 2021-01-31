// Copyright 2020-2021 Miyakowww.
//
// 此源代码的使用受 GNU AFFERO GENERAL PUBLIC LICENSE version 3 许可证的约束, 可以在以下链接找到该许可证.
// Use of this source code is governed by the GNU AGPLv3 license that can be found through the following link.
//
// https://github.com/Miyakowww/CocoaFramework/blob/main/LICENSE

using System.Collections.Immutable;

namespace CocoaFramework.Support
{
    public static class BotAuth
    {
        public static long Owner { get; private set; }
        public static ImmutableArray<long> Admin { get; private set; } = ImmutableArray.Create<long>();

        internal static void Init()
        {
            Owner = BotReg.GetLong("CORE/OWNER", 0);
            Admin = DataManager.LoadData<ImmutableArray<long>?>("AdminList").Result ?? Admin;
        }

        public static bool HasOwner => Owner != 0;
        public static bool IsOwner(long qq) => Owner == qq;
        public static bool IsAdmin(long qq) => IsOwner(qq) || Admin.Contains(qq);

        public static int AuthLevel(long qq)
        {
            if (IsOwner(qq))
            {
                return 2;
            }
            else if (IsAdmin(qq))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public static void SetOwner(long qqID)
        {
            Owner = qqID;
            BotReg.SetLong("CORE/OWNER", qqID);
        }
        public static bool SetAdmin(long qqID)
        {
            if (Admin.Contains(qqID))
            {
                return false;
            }
            else
            {
                Admin = Admin.Add(qqID);
                _ = DataManager.SaveData("AdminList", Admin);
                return true;
            }
        }
        public static bool RemoveAdmin(long qqID)
        {
            if (Admin.Contains(qqID))
            {
                Admin = Admin.Remove(qqID);
                _ = DataManager.SaveData("AdminList", Admin);
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool RemoveAdminAt(int index)
        {
            if (index < Admin.Length && index >= 0)
            {
                Admin = Admin.RemoveAt(index);
                _ = DataManager.SaveData("AdminList", Admin);
                return true;
            }
            return false;
        }
    }
}
