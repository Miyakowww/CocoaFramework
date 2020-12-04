using CocoaFramework.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoaFramework.Core.ProcessingModel
{
    public class Meeting
    {
        private readonly IEnumerator proc;
        private ListeningTarget target;
        private MessageReceiver? receiver;
        private Meeting? child;
        private readonly Meeting root;

        private TimeSpan timeout = TimeSpan.Zero;
        private int counter = 0;
        private bool running = false;
        private bool finished = false;

        private Meeting(ListeningTarget target, IEnumerator proc)
        {
            this.target = target;
            this.proc = proc;
            root = this;
        }
        private Meeting(ListeningTarget target, IEnumerator proc, Meeting root)
        {
            this.target = target;
            this.proc = proc;
            this.root = root;
        }

        public LockStatus Run(MessageSource? src, QMessage? msg)
        {
            if (finished)
            {
                return LockStatus.Continue;
            }
            if (src is not null && !target.Fit(src))
            {
                return LockStatus.Continue;
            }
            running = true;
            if (child is not null)
            {
                LockStatus status = child.Run(src, msg);
                if (status == LockStatus.Finished)
                {
                    child = null;
                }
                else
                {
                    running = false;
                    return status;
                }
            }
            if (receiver is not null)
            {
                receiver.source = src;
                receiver.message = msg;
                receiver.IsTimeout = src is null && msg is null;
            }
            if (proc.MoveNext())
            {
                if (proc.Current is MessageReceiver rec)
                {
                    receiver = rec;
                    LockStatus status = Run(src, msg);
                    running = false;
                    return status;
                }
                if (proc.Current is ListeningTarget tgt)
                {
                    target = tgt;
                }
                if (proc.Current is TimeSpan tout)
                {
                    timeout = tout;
                    LockStatus status = Run(src, msg);
                    running = false;
                    return status;
                }
                if (proc.Current is NotFit)
                {
                    running = false;
                    return LockStatus.Continue;
                }
                if (proc.Current is IEnumerator subm)
                {
                    Meeting m = new Meeting(target, subm, root);
                    LockStatus status = m.Run(src, msg);
                    if (status != LockStatus.Finished)
                    {
                        counter++;
                        child = m;
                        running = false;
                        return status;
                    }
                    else
                    {
                        status = Run(src, msg);
                        running = false;
                        return status;
                    }
                }
                counter++;
                if (timeout > TimeSpan.Zero)
                {
                    int count = counter;
                    new Task(async () =>
                    {
                        await Task.Delay(timeout);
                        await Task.Delay(100);
                        if (counter == count && !running)
                        {
                            root.Run(null, null);
                        }
                    }).Start();
                }
                running = false;
                return LockStatus.NotFinished;
            }
            else
            {
                counter++;
                running = false;
                finished = true;
                return LockStatus.Finished;
            }
        }

        public static void Start(MessageSource src, IEnumerator proc)
        {
            if (src is null)
            {
                return;
            }
            Meeting m = new Meeting(ListeningTarget.FromTarget(src)!, proc);
            if (m.Run(src, null) != LockStatus.Finished)
            {
                ModuleCore.AddLock(m.Run);
            }
        }
    }
}
