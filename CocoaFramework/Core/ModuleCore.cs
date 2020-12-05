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
        private static readonly List<Func<MessageSource, QMessage, LockState>> locks = new();

        [Obsolete("请不要手动进行初始化")]
        public static void Init(Assembly assembly)
        {
            List<BotModuleBase> modules = new();
            Type[] types = assembly.GetTypes();
            foreach (var t in types)
            {
                if (t.BaseType == typeof(BotModuleBase))
                {
                    DisabledAttribute? d = t.GetCustomAttribute<DisabledAttribute>();
                    if (d is not null)
                    {
                        continue;
                    }
                    BotModuleAttribute? m = t.GetCustomAttribute<BotModuleAttribute>();
                    if (m is not null)
                    {
                        BotModuleBase? module = Activator.CreateInstance(t) as BotModuleBase;
                        if (module is not null)
                        {
                            module.name = m.name;
                            module.level = m.level;
                            module.privateAvailable = m.privateAvailable;
                            module.groupAvailable = m.groupAvailable;
                            module.showOnModuleList = m.showOnModuleList;
                            module.processLevel = m.processLevel;
                            module.InitData();
                            module.Init();
                            modules.Add(module);

                            routes.Add(module, new List<RegexRouteInfo>());
                            MethodInfo[] methods = module.GetType().GetMethods();
                            foreach (var method in methods)
                            {
                                d = method.GetCustomAttribute<DisabledAttribute>();
                                if (d is not null)
                                {
                                    continue;
                                }
                                RegexRouteAttribute[] rs = method.GetCustomAttributes<RegexRouteAttribute>().ToArray();
                                if (rs is not null && rs.Length > 0)
                                {
                                    Regex[] regexs = new Regex[rs.Length];
                                    for (int i = 0; i < rs.Length; i++)
                                    {
                                        regexs[i] = rs[i].regex;
                                    }
                                    routes[module].Add(new RegexRouteInfo(module, method, regexs));
                                }
                            }
                        }
                    }
                }
            }
            modules.Sort((a, b) => a.processLevel.CompareTo(b.processLevel));
            Modules = ImmutableArray.Create(modules.ToArray());
        }

        /// <summary>
        /// -2: lock true
        /// -1: false
        /// 0+: moduleID
        /// </summary>
        [Obsolete("请不要手动调用此方法")]
        public static int Run(MessageSource source, QMessage msg)
        {
            // Lock Run
            for (int i = locks.Count - 1; i >= 0; i--)
            {
                LockState state = locks[i](source, msg);
                if (state.Check(0b10))
                {
                    locks.RemoveAt(i);
                }
                if (state.Check(0b01))
                {
                    return -2;
                }
            }

            // Module Run
            for (int i = 0; i < Modules.Length; i++)
            {
                BotModuleBase m = Modules[i];
                if (!m.Enabled)
                {
                    continue;
                }
                bool auth = source.AuthLevel >= m.level && m.UActive(source.user.ID);
                if (auth && source.IsGroup && !(m.groupAvailable && m.GActive(source.group!.ID)))
                {
                    auth = false;
                }
                if (auth && !source.IsGroup && !m.privateAvailable)
                {
                    auth = false;
                }
                if (auth)
                {
                    try
                    {
                        foreach (var r in routes[m])
                        {
                            if (r.Run(source, msg))
                            {
                                m.AddUsage();
                                return i;
                            }
                        }
                        bool stat = m.Run(source, msg);
                        if (stat)
                        {
                            m.AddUsage();
                            return i;
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Module Run Error: {m.name}", e);
                    }
                }
            }
            return -1;
        }


        public static void AddLock(Func<MessageSource, QMessage, LockState> lockRun)
            => locks.Add(lockRun);

        public static void AddLock(Func<MessageSource, QMessage, LockState> lockRun, Func<MessageSource, bool> predicate)
            => locks.Add(new MessageLock(lockRun, predicate, TimeSpan.Zero, null).Run);

        public static void AddLock(Func<MessageSource, QMessage, LockState> lockRun, ListeningTarget target)
            => locks.Add(new MessageLock(lockRun, target.Fit, TimeSpan.Zero, null).Run);

        public static void AddLock(Func<MessageSource, QMessage, LockState> lockRun, MessageSource source)
            => locks.Add(new MessageLock(lockRun, s => s.Equals(source), TimeSpan.Zero, null).Run);

        public static void AddLock(Func<MessageSource, QMessage, LockState> lockRun, Func<MessageSource, bool> predicate, TimeSpan timeout)
            => locks.Add(new MessageLock(lockRun, predicate, timeout, null).Run);

        public static void AddLock(Func<MessageSource, QMessage, LockState> lockRun, ListeningTarget target, TimeSpan timeout)
            => locks.Add(new MessageLock(lockRun, target.Fit, timeout, null).Run);

        public static void AddLock(Func<MessageSource, QMessage, LockState> lockRun, MessageSource source, TimeSpan timeout)
            => locks.Add(new MessageLock(lockRun, s => s.Equals(source), timeout, null).Run);

        public static void AddLock(Func<MessageSource, QMessage, LockState> lockRun, Func<MessageSource, bool> predicate, TimeSpan timeout, Action onTimeout)
            => locks.Add(new MessageLock(lockRun, predicate, timeout, onTimeout).Run);

        public static void AddLock(Func<MessageSource, QMessage, LockState> lockRun, ListeningTarget target, TimeSpan timeout, Action onTimeout)
            => locks.Add(new MessageLock(lockRun, target.Fit, timeout, onTimeout).Run);

        public static void AddLock(Func<MessageSource, QMessage, LockState> lockRun, MessageSource source, TimeSpan timeout, Action onTimeout)
            => locks.Add(new MessageLock(lockRun, s => s.Equals(source), timeout, onTimeout).Run);


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
                    new Task(async () =>
                    {
                        await Task.Delay(this.timeout);
                        if (counter == count && !running)
                        {
                            locks.Remove(Run);
                            this.onTimeout?.Invoke();
                        }
                    }).Start();
                }
            }

            public LockState Run(MessageSource src, QMessage msg)
            {
                if (timeout > TimeSpan.Zero && DateTime.Now - lastRun > timeout)
                {
                    return LockState.Continue;
                }
                running = true;
                if (predicate(src))
                {
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
                                await Task.Delay(100);
                                if (counter == count && !running)
                                {
                                    locks.Remove(Run);
                                    onTimeout?.Invoke();
                                }
                            }).Start();
                        }
                    }
                    running = false;
                    return state;
                }
                else
                {
                    running = false;
                    return LockState.Continue;
                }
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
            private readonly bool isValueType;
            private readonly bool isVoid;

            public RegexRouteInfo(BotModuleBase module, MethodInfo route, Regex[] regexs)
            {
                this.module = module;
                this.route = route;
                this.regexs = regexs;
                ParameterInfo[] parameters = route.GetParameters();
                argCount = parameters.Length;
                argsIndex = new List<(int gnum, int argIndex)>[regexs.Length];
                srcIndex = -1;
                msgIndex = -1;
                isEnumerator = route.ReturnType == typeof(IEnumerator);
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
                for (int i = 0; i < regexs.Length; i++)
                {
                    argsIndex[i] = new List<(int gnum, int argIndex)>();
                    foreach (var gname in regexs[i].GetGroupNames())
                    {
                        for (int j = 0; j < argCount; j++)
                        {
                            if (parameters[j].Name == gname && parameters[j].ParameterType == typeof(string))
                            {
                                argsIndex[i].Add((regexs[i].GroupNumberFromName(gname), j));
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
                    else
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
        public string? name;
        public int level;
        public bool privateAvailable;
        public bool groupAvailable;
        public bool showOnModuleList;
        public int processLevel;

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

        public BotModuleData? ModuleData;

        private readonly List<FieldInfo> fields = new();
        private string? TypeName;

        public bool ActivityOverrode => GetType().GetMethod("GActive", new Type[] { typeof(long) })?.DeclaringType != typeof(BotModuleBase);

        public void InitData()
        {
            foreach (var f in GetType().GetFields())
            {
                if (f.GetCustomAttributes<ModuleDataAttribute>().Any())
                {
                    fields.Add(f);
                }
            }
            TypeName = GetType().Name;
            enabled = BotReg.GetBool($"MODULE/{TypeName}/ENABLED", true);
            LoadData();
        }
        public void LoadData()
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
            foreach (var f in fields)
            {
                object? val = DataManager.LoadData($@"ModuleData\{TypeName}\Field_{f.Name}", f.FieldType).Result;
                if (val is not null)
                {
                    f.SetValue(this, val);
                }
            }
        }
        public void SaveData()
        {
            _ = DataManager.SaveData($@"ModuleData\{TypeName}\.ModuleData", ModuleData);
            foreach (var f in fields)
            {
                _ = DataManager.SaveData($@"ModuleData\{TypeName}\Field_{f.Name}", f.GetValue(this));
            }
        }
        public bool SetGroup(long gid, bool active)
        {
            if (active)
            {
                if (!ModuleData!.ActiveGroup.Contains(gid))
                {
                    ModuleData.ActiveGroup.Add(gid);
                    SaveData();
                    return true;
                }
            }
            else
            {
                if (ModuleData!.ActiveGroup.Contains(gid))
                {
                    ModuleData.ActiveGroup.Remove(gid);
                    SaveData();
                    return true;
                }
            }
            return false;
        }
        public bool SetUser(long uid, bool active)
        {
            if (!active)
            {
                if (!ModuleData!.BanUser.Contains(uid))
                {
                    ModuleData.BanUser.Add(uid);
                    SaveData();
                    return true;
                }
            }
            else
            {
                if (ModuleData!.BanUser.Contains(uid))
                {
                    ModuleData.BanUser.Remove(uid);
                    SaveData();
                    return true;
                }
            }
            return false;
        }

        public void AddUsage()
        {
            int deltaDay = (int)(new DateTime(
                    DateTime.Now.Year,
                    DateTime.Now.Month,
                    DateTime.Now.Day)
                - new DateTime(
                    ModuleData!.LastStatistics.Year,
                    ModuleData.LastStatistics.Month,
                    ModuleData.LastStatistics.Day)
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
            SaveData();
        }
        public int GetUsage(int range)
        {
            if (range >= 30)
            {
                return ModuleData!.Usage.Sum();
            }
            int count = 0;
            for (int i = 0; i < range; i++)
            {
                count += ModuleData!.Usage[i];
            }
            return count;
        }

        public virtual void Init() { }
        public virtual bool Run(MessageSource source, QMessage msg) { return false; }
        public virtual bool GActive(long groupID)
            => ModuleData!.ActiveGroup.Contains(groupID);
        public bool UActive(long userID)
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
