using CocoaFramework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoaFramework.Core.ProcessingModel
{
    public class MessageReceiver
    {
        public MessageSource? source;
        public QMessage? message;
        public bool IsTimeout;
    }
}
