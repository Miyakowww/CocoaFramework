// Copyright 2020-2021 Miyakowww.
//
// 此源代码的使用受 GNU AFFERO GENERAL PUBLIC LICENSE version 3 许可证的约束, 可以在以下链接找到该许可证.
// Use of this source code is governed by the GNU AGPLv3 license that can be found through the following link.
//
// https://github.com/Miyakowww/CocoaFramework/blob/main/LICENSE

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocoaFramework.Support;
using Mirai_CSharp.Models;

namespace CocoaFramework.Model
{
    public class QMessage
    {
        public readonly ImmutableArray<IMessageBase> chain;
        public int ID { get; }
        public DateTime Time { get; }
        public string PlainText { get; }

        public QMessage(IMessageBase[] _chain)
        {
            if (_chain.Length < 2 || _chain[0] is not SourceMessage src)
            {
                throw new ArgumentException("Invalid message chain.");
            }

            ID = src.Id;
            Time = src.Time;
            chain = ImmutableArray.Create(_chain[1..]);
            StringBuilder sb = new();
            for (int i = 1; i < _chain.Length; i++)
            {
                if (_chain[i] is PlainMessage pm)
                {
                    sb.Append(pm.Message);
                }
                else if (_chain[i] is FaceMessage fm)
                {
                    sb.Append($@"\f{fm.Id:d3}");
                }
            }
            PlainText = sb.ToString();
        }

        public T[] GetSubMessages<T>()
            => chain
            .Where(m => m is T)
            .Select(m => (T)m)
            .ToArray();

        public Task RevokeMessageAsync() => BotAPI.RevokeMessageAsync(ID);
        public void RevokeMessage() => BotAPI.RevokeMessageAsync(ID);

        public override string ToString()
            => PlainText;
        public static implicit operator string(QMessage msg)
            => msg.PlainText;
    }
}
