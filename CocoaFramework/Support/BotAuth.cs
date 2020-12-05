using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CocoaFramework.Support
{
    public static class BotAuth
    {
        public static long Owner { get; private set; }
        public static ImmutableArray<long> Admin { get; private set; }

        public static void Init()
        {
            Owner = BotReg.GetLong("CORE/OWNER", 0);
            Admin = ImmutableArray.Create(DataManager.LoadData<long[]>("AdminList").Result ?? Array.Empty<long>());
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

        public static void SetOwner(long qid)
        {
            Owner = qid;
        }
        public static bool SetAdmin(long qid)
        {
            if (Admin.Contains(qid))
            {
                return false;
            }
            else
            {
                Admin = Admin.Add(qid);
                return true;
            }
        }
        public static bool RemoveAdmin(long qid)
        {
            if (Admin.Contains(qid))
            {
                Admin = Admin.Remove(qid);
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
