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
using System.Text.RegularExpressions;
using CocoaFramework.Model;

namespace CocoaFramework.Core.Route
{
    internal class RegexRouteInfo : IRouteInfo
    {
        public BotModuleBase module;
        public MethodInfo route;
        public Regex[] regexs;

        public int srcIndex;
        public int msgIndex;
        public int argCount;
        public List<(int gnum, int argIndex, int paraType)>[] argsIndex;

        private readonly RouteResultProcessor? processor;

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
            argsIndex = new List<(int gnum, int argIndex, int paraType)>[regexs.Length];
            srcIndex = -1;
            msgIndex = -1;

            if (route.ReturnType == typeof(IEnumerator))
            {
                processor = RouteResultProcessors.Enumerator;
            }
            else if (route.ReturnType == typeof(IEnumerable))
            {
                processor = RouteResultProcessors.Enumerable;
            }
            else if (route.ReturnType == typeof(string))
            {
                processor = RouteResultProcessors.String;
            }
            else if (route.ReturnType == typeof(StringBuilder))
            {
                processor = RouteResultProcessors.StringBuilder;
            }

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
                string[] gnames = regexs[reId].GetGroupNames();
                for (int paraId = 0; paraId < argCount; paraId++)
                {
                    string paraName = parameters[paraId].Name!;
                    if (gnames.Contains(paraName))
                    {
                        Type paraType = parameters[paraId].ParameterType;

                        // typeof(xxx) 不是常量，不能使用 switch
                        if (paraType == typeof(string))
                        {
                            argsIndex[reId].Add((regexs[reId].GroupNumberFromName(paraName), paraId, 0));
                        }
                        else if (paraType == typeof(string[]))
                        {
                            argsIndex[reId].Add((regexs[reId].GroupNumberFromName(paraName), paraId, 1));
                        }
                        else if (paraType == typeof(List<string>))
                        {
                            argsIndex[reId].Add((regexs[reId].GroupNumberFromName(paraName), paraId, 2));
                        }
                    }
                }

            }
        }
        public bool Run(MessageSource src, QMessage msg)
        {
            if (string.IsNullOrEmpty(msg.PlainText))
            {
                return false;
            }

            (Match match, var index) = regexs
                .Select((r, i) => (match: r.Match(msg.PlainText), index: argsIndex[i]))
                .FirstOrDefault(t => t.match.Success);

            if (match is null)
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
            foreach (var (gnum, argIndex, paraType) in index)
            {
                args[argIndex] = paraType switch
                {
                    0 => match.Groups[gnum].Value,
                    1 => match.Groups[gnum].Captures.ToArray(),
                    2 => match.Groups[gnum].Captures.ToList(),
                    _ => null
                };
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

            if (processor is not null)
            {
                return processor(src, result);
            }
            else if (isValueType)
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
