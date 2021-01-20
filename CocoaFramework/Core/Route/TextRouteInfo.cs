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

            for (int i = 0; i < texts.Length; i++)
            {
                bool match;
                if (ignoreCases[i])
                {
                    match = msg.PlainText.ToLower() == texts[i].ToLower();
                }
                else
                {
                    match = msg.PlainText == texts[i];
                }
                if (!match)
                {
                    continue;
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
                if (isEnumerator)
                {
                    IEnumerator? meeting;
                    if (isThreadSafe)
                    {
                        lock (_lock)
                        {
                            meeting = route.Invoke(module, args) as IEnumerator;
                        }
                    }
                    else
                    {
                        meeting = route.Invoke(module, args) as IEnumerator;
                    }
                    if (meeting is null)
                    {
                        return false;
                    }
                    Meeting.Start(src, meeting);
                    return true;
                }
                if (isEnumerable)
                {
                    IEnumerable? meeting;
                    if (isThreadSafe)
                    {
                        lock (_lock)
                        {
                            meeting = route.Invoke(module, args) as IEnumerable;
                        }
                    }
                    else
                    {
                        meeting = route.Invoke(module, args) as IEnumerable;
                    }
                    if (meeting is null)
                    {
                        return false;
                    }
                    Meeting.Start(src, meeting);
                    return true;
                }
                if (isString)
                {
                    string? res;
                    if (isThreadSafe)
                    {
                        lock (_lock)
                        {
                            res = route.Invoke(module, args) as string;
                        }
                    }
                    else
                    {
                        res = route.Invoke(module, args) as string;
                    }
                    if (string.IsNullOrEmpty(res))
                    {
                        return false;
                    }
                    src.Send(res);
                    return true;
                }
                if (isStringBuilder)
                {
                    StringBuilder? res;
                    if (isThreadSafe)
                    {
                        lock (_lock)
                        {
                            res = route.Invoke(module, args) as StringBuilder;
                        }
                    }
                    else
                    {
                        res = route.Invoke(module, args) as StringBuilder;
                    }
                    if (res is null || res.Length <= 0)
                    {
                        return false;
                    }
                    src.Send(res.ToString());
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
