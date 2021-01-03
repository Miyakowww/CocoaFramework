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
        public string Name { get; }
        public int Level { get; }
        public bool PrivateAvailable { get; }
        public bool GroupAvailable { get; }
        public bool ShowOnModuleList { get; set; }
        public int ProcessLevel { get; set; }
        public BotModuleAttribute(string name, int level, bool privateAvailable, bool groupAvailable)
        {
            Name = name;
            Level = level;
            PrivateAvailable = privateAvailable;
            GroupAvailable = groupAvailable;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class RegexRouteAttribute : Attribute
    {
        public Regex regex;
        public RegexRouteAttribute(string pattern)
        {
            regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }
        public RegexRouteAttribute(string pattern, RegexOptions options)
        {
            regex = new Regex(pattern, options | RegexOptions.Compiled);
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class BotServiceAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class HostedDataAttribute : Attribute { }
}
