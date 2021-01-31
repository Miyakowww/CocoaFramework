// Copyright 2020-2021 Miyakowww.
//
// 此源代码的使用受 GNU AFFERO GENERAL PUBLIC LICENSE version 3 许可证的约束, 可以在以下链接找到该许可证.
// Use of this source code is governed by the GNU AGPLv3 license that can be found through the following link.
//
// https://github.com/Miyakowww/CocoaFramework/blob/main/LICENSE

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using CocoaFramework.Model;
using CocoaFramework.Support;
using Mirai_CSharp.Models;

namespace CocoaFramework.Core
{
    public static class MiddlewareCore
    {
        public static ImmutableArray<BotMiddlewareBase> Middlewares { get; private set; }

        internal static void Init(BotMiddlewareBase[] middlewares)
        {
            Middlewares = ImmutableArray.Create(middlewares);

            foreach (var m in Middlewares)
            {
                m.onMessageIsThreadSafe = m.GetType().GetMethod(nameof(BotMiddlewareBase.OnMessage))?.GetCustomAttribute<ThreadSafeAttribute>() is not null;
                m.onSendIsThreadSafe = m.GetType().GetMethod(nameof(BotMiddlewareBase.OnSend))?.GetCustomAttribute<ThreadSafeAttribute>() is not null;
                m.InitData();
                m.Init();
            }
        }

        internal static bool OnMessage(ref MessageSource src, ref QMessage msg)
        {
            foreach (var middleware in Middlewares)
            {
                try
                {
                    bool ctn = middleware.OnMessageSafe(ref src, ref msg);
                    if (!ctn)
                    {
                        return false;
                    }
                }
                catch (Exception e)
                {
                    throw new Exception($"Middleware Run Error: {middleware.GetType()}", e);
                }
            }
            return true;
        }
        internal static bool OnSend(ref long id, ref bool isGroup, ref IMessageBase[] chain, ref int? quote)
        {
            foreach (var m in Middlewares)
            {
                bool ctn = m.OnSendSafe(ref id, ref isGroup, ref chain, ref quote);
                if (!ctn)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public abstract class BotMiddlewareBase
    {
        protected internal virtual void Init() { }
        [ThreadSafe]
        protected internal virtual bool OnMessage(ref MessageSource src, ref QMessage msg) => true;
        [ThreadSafe]
        protected internal virtual bool OnSend(ref long id, ref bool isGroup, ref IMessageBase[] chain, ref int? quote) => true;

        private readonly List<FieldInfo> hostedFields = new();
        private string? TypeName;

        internal bool onMessageIsThreadSafe;
        internal readonly object onMessageLock = new();
        internal bool onSendIsThreadSafe;
        internal readonly object onSendLock = new();

        internal bool OnMessageSafe(ref MessageSource src, ref QMessage msg)
        {
            if (onMessageIsThreadSafe)
            {
                return OnMessage(ref src, ref msg);
            }
            else
            {
                lock (onMessageLock)
                {
                    return OnMessage(ref src, ref msg);
                }
            }
        }
        internal bool OnSendSafe(ref long id, ref bool isGroup, ref IMessageBase[] chain, ref int? quote)
        {
            if (onSendIsThreadSafe)
            {
                return OnSend(ref id, ref isGroup, ref chain, ref quote);
            }
            else
            {
                lock (onSendLock)
                {
                    return OnSend(ref id, ref isGroup, ref chain, ref quote);
                }
            }
        }

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
            LoadData();
        }
        internal void LoadData()
        {
            if (!Directory.Exists($@"{DataManager.dataPath}MiddlewareData/{TypeName}"))
            {
                Directory.CreateDirectory($@"{DataManager.dataPath}MiddlewareData/{TypeName}");
            }
            foreach (var f in hostedFields)
            {
                object? val = DataManager.LoadData($@"MiddlewareData/{TypeName}/Field_{f.Name}", f.FieldType).Result;
                if (val is not null)
                {
                    f.SetValue(this, val);
                }
            }
        }
        internal void SaveData()
        {
            foreach (var f in hostedFields)
            {
                _ = DataManager.SaveData($@"MiddlewareData/{TypeName}/Field_{f.Name}", f.GetValue(this));
            }
        }
    }
}
