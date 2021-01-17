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

        private readonly object _lock = new();

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

        public LockState Run(MessageSource? src, QMessage? msg)
        {
            lock (_lock)
            {
                return InternalRun(src, msg);
            }
        }

        private LockState InternalRun(MessageSource? src, QMessage? msg)
        {
            if (finished)
            {
                return LockState.Continue;
            }
            if (src is not null && !target.Fit(src))
            {
                return LockState.Continue;
            }
            running = true;
            if (child is not null)
            {
                LockState state = child.InternalRun(src, msg);
                if (state == LockState.Finished)
                {
                    child = null;
                }
                else
                {
                    running = false;
                    return state;
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
                    LockState state = InternalRun(src, msg);
                    running = false;
                    return state;
                }
                if (proc.Current is ListeningTarget tgt)
                {
                    target = tgt;
                }
                if (proc.Current is TimeSpan tout)
                {
                    timeout = tout;
                    LockState state = InternalRun(src, msg);
                    running = false;
                    return state;
                }
                if (proc.Current is NotFit)
                {
                    running = false;
                    return LockState.Continue;
                }
                if (proc.Current is IEnumerator subm)
                {
                    Meeting m = new Meeting(target, subm, root);
                    LockState state = m.InternalRun(src, msg);
                    if (state != LockState.Finished)
                    {
                        counter++;
                        child = m;
                        running = false;
                        return state;
                    }
                    else
                    {
                        state = InternalRun(src, msg);
                        running = false;
                        return state;
                    }
                }
                if (proc.Current is IEnumerable subma)
                {
                    Meeting m = new Meeting(target, subma.GetEnumerator(), root);
                    LockState state = m.InternalRun(src, msg);
                    if (state != LockState.Finished)
                    {
                        counter++;
                        child = m;
                        running = false;
                        return state;
                    }
                    else
                    {
                        state = InternalRun(src, msg);
                        running = false;
                        return state;
                    }
                }
                counter++;
                if (timeout > TimeSpan.Zero)
                {
                    int count = counter;
                    Task.Run(async () =>
                    {
                        await Task.Delay(timeout);
                        if (counter == count && !running)
                        {
                            root.Run(null, null);
                        }
                    });
                }
                running = false;
                return LockState.NotFinished;
            }
            else
            {
                counter++;
                running = false;
                finished = true;
                return LockState.Finished;
            }
        }

        public static void Start(MessageSource src, IEnumerable proc)
        {
            Start(src, proc.GetEnumerator());
        }
        public static void Start(MessageSource src, IEnumerator proc)
        {
            if (src is null)
            {
                return;
            }
            Meeting m = new Meeting(ListeningTarget.FromTarget(src)!, proc);
            if (m.InternalRun(src, null) != LockState.Finished)
            {
                ModuleCore.AddLock(m.Run);
            }
        }
        public static void Start(ListeningTarget target, IEnumerable proc)
        {
            Start(target, proc.GetEnumerator());
        }
        public static void Start(ListeningTarget target, IEnumerator proc)
        {
            if (target is null)
            {
                return;
            }
            Meeting m = new Meeting(target, proc);
            if (m.InternalRun(null, null) != LockState.Finished)
            {
                ModuleCore.AddLock(m.Run);
            }
        }
    }
}
