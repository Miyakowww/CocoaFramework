// Copyright 2020-2021 Miyakowww.
//
// 此源代码的使用受 GNU AFFERO GENERAL PUBLIC LICENSE version 3 许可证的约束, 可以在以下链接找到该许可证.
// Use of this source code is governed by the GNU AGPLv3 license that can be found through the following link.
//
// https://github.com/Miyakowww/CocoaFramework/blob/main/LICENSE

using System;

namespace CocoaFramework.Core.ProcessingModel
{
    public class NotFit
    {
        private NotFit() { }
        internal bool remove { get; init; }

        [Obsolete("Use NotFit.Continue instead.")]
        public static NotFit Instance => Continue;

        public static readonly NotFit Continue = new() {remove = false};
        public static readonly NotFit Stop = new() {remove = true};
    }
}