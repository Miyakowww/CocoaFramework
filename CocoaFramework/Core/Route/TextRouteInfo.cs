// Copyright 2020-2021 Miyakowww.
//
// 此源代码的使用受 GNU AFFERO GENERAL PUBLIC LICENSE version 3 许可证的约束, 可以在以下链接找到该许可证.
// Use of this source code is governed by the GNU AGPLv3 license that can be found through the following link.
//
// https://github.com/Miyakowww/CocoaFramework/blob/main/LICENSE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CocoaFramework.Core.ProcessingModel;
using CocoaFramework.Model;

namespace CocoaFramework.Core.Route
{
    internal class TextRouteInfo : IRouteInfo
    {
        public BotModuleBase module;
        public MethodInfo route;
        public string[] texts;
        public bool[] ignoreCases;

        public int srcIndex;
        public int msgIndex;
        public int argCount;
        public List<(int gnum, int argIndex, int paraType)>[] argsIndex;

        private readonly bool isEnumerator;
        private readonly bool isEnumerable;
        private readonly bool isString;
        private readonly bool isStringBuilder;
        private readonly bool isValueType;
        private readonly bool isVoid;
        private readonly bool isThreadSafe;

        private readonly object _lock = new();

        public TextRouteInfo(BotModuleBase module, MethodInfo route, string[] texts, bool[] ignoreCases, bool isThreadSafe)
        {
            this.module = module;
            this.route = route;
            this.texts = texts;
            this.ignoreCases = ignoreCases;
            this.isThreadSafe = isThreadSafe;

            ParameterInfo[] parameters = route.GetParameters();
            argCount = parameters.Length;
            argsIndex = new List<(int gnum, int argIndex, int paraType)>[texts.Length];
            srcIndex = -1;
            msgIndex = -1;
            isEnumerator = route.ReturnType == typeof(IEnumerator);
            isEnumerable = route.ReturnType == typeof(IEnumerable);
            isString = route.ReturnType == typeof(string);
            isStringBuilder = route.ReturnType == typeof(StringBuilder);
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
        }
        public bool Run(MessageSource src, QMessage msg)
        {
            if (string.IsNullOrEmpty(msg.PlainText))
                return false;

            if (!texts.Where((t, i) => ignoreCases[i] ?
             msg.PlainText.ToLower() == t.ToLower() :
             msg.PlainText == t)
                .Any())
            {
                return false;
            }

            object?[] args = new object?[argCount];
            if (srcIndex != -1)
            {
                args[srcIndex] = src;
            }
            if (msgIndex != -1)
            {
                args[msgIndex] = msg;
            }

            object? result;
            if (isThreadSafe)
            {
                lock (_lock)
                {
                    result = route.Invoke(module, args);
                }
            }
            else
            {
                result = route.Invoke(module, args);
            }

            if (isEnumerator)
            {
                if (result is not IEnumerator meeting)
                {
                    return false;
                }
                Meeting.Start(src, meeting);
                return true;
            }
            if (isEnumerable)
            {
                if (result is not IEnumerable meeting)
                {
                    return false;
                }
                Meeting.Start(src, meeting);
                return true;
            }
            if (isString)
            {
                string? res = result as string;
                if (string.IsNullOrEmpty(res))
                {
                    return false;
                }
                src.Send(res);
                return true;
            }
            if (isStringBuilder)
            {
                if (result is not StringBuilder res || res.Length <= 0)
                {
                    return false;
                }
                src.Send(res.ToString());
                return true;
            }

            if (isValueType)
            {
                return !result?.Equals(Activator.CreateInstance(route.ReturnType)) ?? false;
            }
            else if (isVoid)
            {
                return true;
            }
            else
            {
                return result is not null;
            }
        }
    }
}
