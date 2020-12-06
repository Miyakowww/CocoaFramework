using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CocoaFramework.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class DisabledAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class BotComponentAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class BotModuleAttribute : Attribute
    {
        public string name;
        public int level;
        public bool privateAvailable;
        public bool groupAvailable;
        public bool showOnModuleList;
        public int processLevel;
        public BotModuleAttribute(string name, int level, bool privateAvailable, bool groupAvailable)
            : this(name, level, privateAvailable, groupAvailable, true, 0) { }
        public BotModuleAttribute(string name, int level, bool privateAvailable, bool groupAvailable, bool showOnModuleList)
            : this(name, level, privateAvailable, groupAvailable, showOnModuleList, 0) { }
        public BotModuleAttribute(string name, int level, bool privateAvailable, bool groupAvailable, int processLevel)
            : this(name, level, privateAvailable, groupAvailable, true, processLevel) { }
        public BotModuleAttribute(string name, int level, bool privateAvailable, bool groupAvailable, bool showOnModuleList, int processLevel)
        {
            this.name = name;
            this.level = level;
            this.privateAvailable = privateAvailable;
            this.groupAvailable = groupAvailable;
            this.showOnModuleList = showOnModuleList;
            this.processLevel = processLevel;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class RegexRouteAttribute : Attribute
    {
        public Regex regex;
        public RegexRouteAttribute(string pattern)
        {
            regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }
        public RegexRouteAttribute(string pattern, RegexOptions options)
        {
            regex = new Regex(pattern, options | RegexOptions.Compiled);
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class BotServiceAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class ModuleDataAttribute : Attribute { }
}
