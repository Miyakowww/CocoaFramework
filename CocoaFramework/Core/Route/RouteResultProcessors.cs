// Copyright 2020-2021 Miyakowww.
//
// 此源代码的使用受 GNU AFFERO GENERAL PUBLIC LICENSE version 3 许可证的约束, 可以在以下链接找到该许可证.
// Use of this source code is governed by the GNU AGPLv3 license that can be found through the following link.
//
// https://github.com/Miyakowww/CocoaFramework/blob/main/LICENSE

using System;
using System.Collections;
using System.Text;
using CocoaFramework.Core.ProcessingModel;
using CocoaFramework.Model;

namespace CocoaFramework.Core.Route
{
    internal delegate bool RouteResultProcessor(MessageSource src, object? result);

    internal static class RouteResultProcessors
    {
        public static RouteResultProcessor? GetProcessor(Type type)
        {
            if (type == typeof(IEnumerator))
            {
                return Enumerator;
            }
            else if (type == typeof(IEnumerable))
            {
                return Enumerable;
            }
            else if (type == typeof(string))
            {
                return String;
            }
            else if (type == typeof(StringBuilder))
            {
                return StringBuilder;
            }
            else
            {
                return null;
            }
        }

        public static bool Enumerator(MessageSource src, object? result)
        {
            if (result is not IEnumerator meeting)
            {
                return false;
            }
            Meeting.Start(src, meeting);
            return true;
        }
        public static bool Enumerable(MessageSource src, object? result)
        {
            if (result is not IEnumerable meeting)
            {
                return false;
            }
            Meeting.Start(src, meeting);
            return true;
        }
        public static bool String(MessageSource src, object? result)
        {
            string? res = result as string;
            if (string.IsNullOrEmpty(res))
            {
                return false;
            }
            src.Send(res);
            return true;
        }
        public static bool StringBuilder(MessageSource src, object? result)
        {
            if (result is not StringBuilder res || res.Length <= 0)
            {
                return false;
            }
            src.Send(res.ToString());
            return true;
        }
    }
}
