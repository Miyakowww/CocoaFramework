// Copyright 2020-2021 Miyakowww.
//
// 此源代码的使用受 GNU AFFERO GENERAL PUBLIC LICENSE version 3 许可证的约束, 可以在以下链接找到该许可证.
// Use of this source code is governed by the GNU AGPLv3 license that can be found through the following link.
//
// https://github.com/Miyakowww/CocoaFramework/blob/main/LICENSE

using System;
using System.Text.RegularExpressions;

namespace CocoaFramework.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DisabledAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class BotComponentAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class)]
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
        public Regex Regex { get; }
        public RegexRouteAttribute(string pattern)
        {
            Regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }
        public RegexRouteAttribute(string pattern, RegexOptions options)
        {
            Regex = new Regex(pattern, options | RegexOptions.Compiled);
        }
    }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class TextRouteAttribute : Attribute
    {
        public string Text { get; }
        public bool IgnoreCase { get; set; } = true;
        public TextRouteAttribute(string text)
        {
            Text = text;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class BotServiceAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class HostedDataAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class ThreadSafeAttribute : Attribute { }
}
