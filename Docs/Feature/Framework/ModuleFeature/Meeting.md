# Meeting

Meeting 是对一段连续的消息处理过程的抽象，即目标的消息不再是独立的指令，而是相互之间存在关联的对话。Meeting 基于 [MessageLock](./MessageLock.md) 实现

<br>

## 开始 Meeting
可以使用 Meeting.Start 开启一次 Meeting，也可以通过 [RegexRoute](./RegexRoute.md) 自动启动

<br>

## yield return
yield return 将作为 Meeting 向管理器传递状态的方式
- 返回值类型
    - [MessageReceiver](./ProcessingModel/MessageReceiver.md)：设置接收消息的接收器，此次返回不会中断执行
    - [ListeningTarget](./ProcessingModel/ListeningTarget.md)：设置消息的监听源，此次返回会中断执行，直到有来自监听目标的消息
    - TimeSpan：设置 Meeting 的超时时长，超时后枚举器将会收到一次超时消息。设为 TimeSpan.Zero 以关闭超时。此次返回不会中断执行
    - [NotFit](./ProcessingModel/NotFit.md)：使管理器返回 [LockState](./MessageLock.md).Continue。此次返回会中断执行，直到有来自监听目标的消息
    - IEnumerator 或 IEnumerable：将给定的枚举器作为子 Meeting，此枚举器的 Next 方法会被立即调用。借助 [GetValue](./ProcessingModel/GetValue.md) 可实现 Meeting 间通信
    - null：此次返回会中断执行，直到有来自监听目标的消息
