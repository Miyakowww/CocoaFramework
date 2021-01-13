using CocoaFramework.Core.ProcessingModel;
using CocoaFramework.Model;
using CocoaFramework.Support;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CocoaFramework.Core
{
    public static class ModuleCore
    {
        public static ImmutableArray<BotModuleBase> Modules { get; private set; }
        private static readonly Dictionary<BotModuleBase, List<RegexRouteInfo>> routes = new();
        private static readonly List<Func<MessageSource, QMessage, LockState>> messageLocks = new();

        internal static void Init(Assembly assembly)
        {
            List<BotModuleBase> modules = new();
            Type[] types = assembly.GetTypes();
            foreach (var t in types)
            {
                if (t.BaseType != typeof(BotModuleBase)) // 提前判断，避免不必要的实例化
                {
                    continue;
                }
                if (t.GetCustomAttribute<DisabledAttribute>() is not null)
                {
                    continue;
                }
                if (t.GetCustomAttribute<BotModuleAttribute>() is not BotModuleAttribute m || m is null)
                {
                    continue;
                }
                if (Activator.CreateInstance(t) is not BotModuleBase module)
                {
                    continue;
                }

                module.Name = m.Name;
                module.Level = m.Level;
                module.PrivateAvailable = m.PrivateAvailable;
                module.GroupAvailable = m.GroupAvailable;
                module.ShowOnModuleList = m.ShowOnModuleList;
                module.ProcessLevel = m.ProcessLevel;
                module.onMessageIsThreadSafe = t.GetMethod("OnMessage")?.GetCustomAttribute<ThreadSafeAttribute>() is not null;
                module.ActivityOverrode = t.GetMethod("GroupActivity", new Type[] { typeof(long) })?.DeclaringType != typeof(BotModuleBase);

                module.InitData();
                module.Init();
                modules.Add(module);

                routes.Add(module, new List<RegexRouteInfo>());
                MethodInfo[] methods = module.GetType().GetMethods();
                foreach (var method in methods)
                {
                    if (method.GetCustomAttribute<DisabledAttribute>() is not null)
                    {
                        continue;
                    }
                    if (method.GetCustomAttributes<RegexRouteAttribute>().ToArray() is not RegexRouteAttribute[] rs || rs.Length <= 0)
                    {
                        continue;
                    }
                    Regex[] regexs = new Regex[rs.Length];
                    for (int i = 0; i < rs.Length; i++)
                    {
                        regexs[i] = rs[i].regex;
                    }
                    routes[module].Add(new RegexRouteInfo(module, method, regexs, method.GetCustomAttribute<ThreadSafeAttribute>() is not null));
                }
            }
            modules.Sort((a, b) => a.ProcessLevel.CompareTo(b.ProcessLevel));
            Modules = modules.ToImmutableArray();
        }

        /// <summary>
        /// -2: lock true <br/>
        /// -1: false <br/>
        /// 0+: moduleID
        /// </summary>
        internal static int OnMessage(MessageSource src, QMessage msg)
        {
            // Lock Run
            // 后加入的先被处理，添加同一个监听对象可以简单地实现“子消息锁”，
            // 也可以在移除并继续时不必考虑循环变量问题
            for (int i = messageLocks.Count - 1; i >= 0; i--)
            {
                LockState state = messageLocks[i](src, msg);
                // 高位表示是否移除，低位表示是否被处理
                if (state.Check(0b10))
                {
                    messageLocks.RemoveAt(i);
                }
                if (state.Check(0b01))
                {
                    return -2;
                }
            }

            // Module Run
            // 需要返回 ID，所以不能用 foreach
            for (int i = 0; i < Modules.Length; i++)
            {
                BotModuleBase m = Modules[i];
                if (!m.Enabled)
                {
                    continue;
                }

                // 权限足够
                bool auth = src.AuthLevel >= m.Level && m.UserActivity(src.user.ID);
                // 群消息时要求群聊可用
                auth &= !src.IsGroup || (m.GroupAvailable && m.GroupActivity(src.group!.ID));
                // 私聊消息时要求私聊可用
                auth &= src.IsGroup || m.PrivateAvailable;

                if (auth)
                {
                    try
                    {
                        foreach (var r in routes[m])
                        {
                            if (r.Run(src, msg))
                            {
                                m.AddUsage();
                                return i;
                            }
                        }
                        if (m.OnMessageSafe(src, msg))
                        {
                            m.AddUsage();
                            return i;
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Module Run Error: {m.Name}", e);
                    }
                }
            }
            return -1;
        }


        public static void AddLock(Func<MessageSource, QMessage, LockState> lockRun)
            => messageLocks.Add(lockRun);

        public static void AddLock(Func<MessageSource, QMessage, LockState> lockRun, Func<MessageSource, bool> predicate)
            => messageLocks.Add(new MessageLock(lockRun, predicate, TimeSpan.Zero, null).Run);

        public static void AddLock(Func<MessageSource, QMessage, LockState> lockRun, ListeningTarget target)
            => messageLocks.Add(new MessageLock(lockRun, target.Fit, TimeSpan.Zero, null).Run);

        public static void AddLock(Func<MessageSource, QMessage, LockState> lockRun, MessageSource src)
            => messageLocks.Add(new MessageLock(lockRun, s => s.Equals(src), TimeSpan.Zero, null).Run);

        public static void AddLock(Func<MessageSource, QMessage, LockState> lockRun, Func<MessageSource, bool> predicate, TimeSpan timeout, Action onTimeout = null!)
            => messageLocks.Add(new MessageLock(lockRun, predicate, timeout, onTimeout).Run);

        public static void AddLock(Func<MessageSource, QMessage, LockState> lockRun, ListeningTarget target, TimeSpan timeout, Action onTimeout = null!)
            => messageLocks.Add(new MessageLock(lockRun, target.Fit, timeout, onTimeout).Run);

        public static void AddLock(Func<MessageSource, QMessage, LockState> lockRun, MessageSource src, TimeSpan timeout, Action onTimeout = null!)
            => messageLocks.Add(new MessageLock(lockRun, s => s.Equals(src), timeout, onTimeout).Run);


        private class MessageLock
        {
            public readonly Func<MessageSource, bool> predicate;
            public readonly Func<MessageSource, QMessage, LockState> run;
            private readonly TimeSpan timeout;
            private readonly Action? onTimeout;
            private int counter = 0;
            private DateTime lastRun;
            private bool running = false;

            public MessageLock(Func<MessageSource, QMessage, LockState> run, Func<MessageSource, bool> predicate, TimeSpan timeout, Action? onTimeout)
            {
                this.predicate = predicate;
                this.run = run;
                this.timeout = timeout;
                this.onTimeout = onTimeout;
                lastRun = DateTime.Now;
                if (timeout > TimeSpan.Zero)
                {
                    int count = counter;
                    Task.Run(async () =>
                    {
                        await Task.Delay(this.timeout);
                        // 与超时判断产生时间差，避免产生边界问题
                        await Task.Delay(100);
                        if (counter == count && !running)
                        {
                            messageLocks.Remove(Run);
                            this.onTimeout?.Invoke();
                        }
                    });
                }
            }

            public LockState Run(MessageSource src, QMessage msg)
            {
                // 超时后直接跳过，之后 Lock 由超时方法移除
                if (timeout > TimeSpan.Zero && DateTime.Now - lastRun > timeout)
                {
                    return LockState.Continue;
                }
                running = true;
                if (!predicate(src))
                {
                    running = false;
                    return LockState.Continue;
                }

                LockState state = run(src, msg);
                if (state.Check(0b10))
                {
                    counter++;
                }
                else if (state.Check(0b01))
                {
                    lastRun = DateTime.Now;
                    counter++;
                    if (timeout > TimeSpan.Zero)
                    {
                        int count = counter;
                        new Task(async () =>
                        {
                            await Task.Delay(timeout);
                            // 与超时判断产生时间差，避免产生边界问题
                            await Task.Delay(100);
                            if (counter == count && !running)
                            {
                                messageLocks.Remove(Run);
                                onTimeout?.Invoke();
                            }
                        }).Start();
                    }
                }
                running = false;
                return state;
            }
        }

        private static bool Check(this LockState state, int flag)
        {
            return ((int)state & flag) != 0;
        }

        private class RegexRouteInfo
        {
            public BotModuleBase module;
            public MethodInfo route;
            public Regex[] regexs;

            public int srcIndex;
            public int msgIndex;
            public int argCount;
            public List<(int gnum, int argIndex)>[] argsIndex;

            private readonly bool isEnumerator;
            private readonly bool isEnumerable;
            private readonly bool isValueType;
            private readonly bool isVoid;
            private readonly bool isThreadSafe;

            private readonly object _lock = new();

            public RegexRouteInfo(BotModuleBase module, MethodInfo route, Regex[] regexs, bool isThreadSafe)
            {
                this.module = module;
                this.route = route;
                this.regexs = regexs;
                this.isThreadSafe = isThreadSafe;

                ParameterInfo[] parameters = route.GetParameters();
                argCount = parameters.Length;
                argsIndex = new List<(int gnum, int argIndex)>[regexs.Length];
                srcIndex = -1;
                msgIndex = -1;
                isEnumerator = route.ReturnType == typeof(IEnumerator);
                isEnumerable = route.ReturnType == typeof(IEnumerable);
                isVoid = route.ReturnType == typeof(void);
                isValueType = route.ReturnType.IsValueType && !isVoid;

                for (int i = 0; i < argCount; i++)
                {
                    if (parameters[i].ParameterType == typeof(MessageSource) && srcIndex == -1)
                    {
                        srcIndex = i;
                    }
                    if (parameters[i].ParameterType == typeof(QMessage) && msgIndex == -1)
                    {
                        msgIndex = i;
                    }
                }

                for (int reId = 0; reId < regexs.Length; reId++)
                {
                    argsIndex[reId] = new();
                    foreach (var gname in regexs[reId].GetGroupNames())
                    {
                        for (int paraId = 0; paraId < argCount; paraId++)
                        {
                            if (parameters[paraId].Name == gname && parameters[paraId].ParameterType == typeof(string))
                            {
                                argsIndex[reId].Add((regexs[reId].GroupNumberFromName(gname), paraId));
                                break;
                            }
                        }
                    }
                }
            }
            public bool Run(MessageSource src, QMessage msg)
            {
                if (string.IsNullOrEmpty(msg.PlainText))
                    return false;

                for (int i = 0; i < regexs.Length; i++)
                {
                    Match match = regexs[i].Match(msg.PlainText);
                    if (!match.Success)
                    {
                        continue;
                    }
                    object[] args = new object[argCount];
                    if (srcIndex != -1)
                    {
                        args[srcIndex] = src;
                    }
                    if (msgIndex != -1)
                    {
                        args[msgIndex] = msg;
                    }
                    foreach (var (gnum, argIndex) in argsIndex[i])
                    {
                        args[argIndex] = match.Groups[gnum].Value;
                    }
                    if (isEnumerator)
                    {
                        Meeting.Start(src, (route.Invoke(module, args) as IEnumerator)!);
                        return true;
                    }
                    if (isEnumerable)
                    {
                        Meeting.Start(src, (route.Invoke(module, args) as IEnumerable)!);
                        return true;
                    }
                    else
                    {
                        if (isThreadSafe)
                        {
                            if (isValueType)
                            {
                                return !route.Invoke(module, args)!.Equals(Activator.CreateInstance(route.ReturnType));
                            }
                            else if (isVoid)
                            {
                                route.Invoke(module, args);
                                return true;
                            }
                            else
                            {
                                return route.Invoke(module, args) is not null;
                            }
                        }
                        else
                        {
                            lock (_lock)
                            {
                                if (isValueType)
                                {
                                    return !route.Invoke(module, args)!.Equals(Activator.CreateInstance(route.ReturnType));
                                }
                                else if (isVoid)
                                {
                                    route.Invoke(module, args);
                                    return true;
                                }
                                else
                                {
                                    return route.Invoke(module, args) is not null;
                                }
                            }
                        }
                    }
                }
                return false;
            }
        }
    }

    public class BotModuleData
    {
        public List<long> ActiveGroup = new();
        public List<long> BanUser = new();

        public DateTime LastStatistics;
        public List<int> Usage = new();
    }

    public abstract class BotModuleBase
    {
        public string? Name { get; internal set; }
        public int Level { get; internal set; }
        public bool PrivateAvailable { get; internal set; }
        public bool GroupAvailable { get; internal set; }
        public bool ShowOnModuleList { get; internal set; }
        public int ProcessLevel { get; internal set; }

        private bool enabled;
        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;
                BotReg.SetBool($"MODULE/{TypeName}/ENABLED", value);
            }
        }

        public BotModuleData? ModuleData { get; set; }

        private readonly List<FieldInfo> hostedFields = new();
        private string? TypeName;

        public bool ActivityOverrode { get; internal set; }

        internal void InitData()
        {
            foreach (var f in GetType().GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (f.GetCustomAttributes<HostedDataAttribute>().Any())
                {
                    hostedFields.Add(f);
                }
            }
            TypeName = GetType().Name;
            enabled = BotReg.GetBool($"MODULE/{TypeName}/ENABLED", true);
            LoadData();
        }
        internal void LoadData()
        {
            if (!Directory.Exists($@"{DataManager.dataPath}ModuleData\{TypeName}"))
            {
                Directory.CreateDirectory($@"{DataManager.dataPath}ModuleData\{TypeName}");
            }
            ModuleData = DataManager.LoadData<BotModuleData>($@"ModuleData\{TypeName}\.ModuleData").Result ?? new BotModuleData
            {
                ActiveGroup = new List<long>(),
                BanUser = new List<long>(),
                LastStatistics = DateTime.Now,
                Usage = new List<int>(new int[30])
            };
            foreach (var f in hostedFields)
            {
                object? val = DataManager.LoadData($@"ModuleData\{TypeName}\Field_{f.Name}", f.FieldType).Result;
                if (val is not null)
                {
                    f.SetValue(this, val);
                }
            }
        }
        internal void SaveData()
        {
            _ = DataManager.SaveData($@"ModuleData\{TypeName}\.ModuleData", ModuleData);
            foreach (var f in hostedFields)
            {
                _ = DataManager.SaveData($@"ModuleData\{TypeName}\Field_{f.Name}", f.GetValue(this));
            }
        }
        public bool SetGroupActivity(long groupID, bool activity)
        {
            if (activity && !ModuleData!.ActiveGroup.Contains(groupID))
            {
                ModuleData.ActiveGroup.Add(groupID);
                SaveData();
                return true;
            }
            else if (!activity && ModuleData!.ActiveGroup.Contains(groupID))
            {
                ModuleData.ActiveGroup.Remove(groupID);
                SaveData();
                return true;
            }
            return false;
        }
        public bool SetUserBan(long qqID, bool banned)
        {
            if (banned && !ModuleData!.BanUser.Contains(qqID))
            {
                ModuleData.BanUser.Add(qqID);
                SaveData();
                return true;
            }
            else if (!banned && ModuleData!.BanUser.Contains(qqID))
            {
                ModuleData.BanUser.Remove(qqID);
                SaveData();
                return true;
            }
            return false;
        }

        internal readonly object usageLock = new();
        internal readonly object onMessageLock = new();

        internal bool onMessageIsThreadSafe;

        internal void AddUsage()
        {
            lock (usageLock)
            {
                int deltaDay = (int)(new DateTime(
                        DateTime.Now.Year,
                        DateTime.Now.Month,
                        DateTime.Now.Day)
                    - new DateTime(
                        ModuleData!.LastStatistics.Year,
                        ModuleData!.LastStatistics.Month,
                        ModuleData!.LastStatistics.Day)
                    ).TotalDays;
                if (deltaDay > 0)
                {
                    ModuleData.Usage.InsertRange(0, new int[deltaDay]);
                    if (ModuleData.Usage.Count > 30)
                    {
                        ModuleData.Usage.RemoveRange(30, ModuleData.Usage.Count - 30);
                    }
                }
                ModuleData.Usage[0]++;
                ModuleData.LastStatistics = DateTime.Now;
            }
            SaveData();
        }
        public int GetUsage(int dayCount)
        {
            lock (usageLock)
            {
                int deltaDay = (int)(new DateTime(
                        DateTime.Now.Year,
                        DateTime.Now.Month,
                        DateTime.Now.Day)
                    - new DateTime(
                        ModuleData!.LastStatistics.Year,
                        ModuleData!.LastStatistics.Month,
                        ModuleData!.LastStatistics.Day)
                    ).TotalDays;
                if (deltaDay > 0)
                {
                    ModuleData.Usage.InsertRange(0, new int[deltaDay]);
                    if (ModuleData.Usage.Count > 30)
                    {
                        ModuleData.Usage.RemoveRange(30, ModuleData.Usage.Count - 30);
                    }
                    ModuleData.LastStatistics = DateTime.Now;
                    SaveData();
                }
                if (dayCount >= 30)
                {
                    return ModuleData!.Usage.Sum();
                }
                int count = 0;
                for (int i = 0; i < dayCount; i++)
                {
                    count += ModuleData!.Usage[i];
                }
                return count;
            }
        }

        protected internal virtual void Init() { }
        [ThreadSafe]
        protected internal virtual bool OnMessage(MessageSource src, QMessage msg) { return false; }

        internal bool OnMessageSafe(MessageSource src, QMessage msg)
        {
            if (onMessageIsThreadSafe)
            {
                return OnMessage(src, msg);
            }
            else
            {
                lock (onMessageLock)
                {
                    return OnMessage(src, msg);
                }
            }
        }

        public virtual bool GroupActivity(long groupID)
            => ModuleData!.ActiveGroup.Contains(groupID);
        public bool UserActivity(long userID)
            => !ModuleData!.BanUser.Contains(userID);
    }

    public enum LockState
    {
        Finished = 0b11,
        NotFinished = 0b01,
        Continue = 0b00,
        ContinueAndRemove = 0b10
    }
}
