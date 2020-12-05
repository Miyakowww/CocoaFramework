using Mirai_CSharp.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace CocoaFramework.Model
{
    public class QMessage
    {
        public readonly ImmutableArray<IMessageBase> chain;
        public readonly int ID;
        public readonly DateTime Time;
        public readonly string PlainText;

        public QMessage(IMessageBase[] _chain)
        {
            if (_chain.Length < 2 || _chain[0] is not SourceMessage src)
                throw new ArgumentException("Invalid message chain.");

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

        public T[] GetMessages<T>()
            => chain
            .Where(m => m is T)
            .Select(m => (T)m)
            .ToArray();

        public override string ToString()
            => PlainText;
        public static implicit operator string(QMessage msg)
            => msg.PlainText;
    }
}
