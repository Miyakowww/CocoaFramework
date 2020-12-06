# MessageLock

消息锁是一种特殊的消息处理方式，会将指定来源的消息移交指定的方法处理，优先级高于模块

<br>

## 添加 MessageLock
可以使用 ModuleCore.AddLock 方法添加 MessageLock
- 参数
    - lockRun：消息锁的目标方法
    - predicate 或 target 或 src：可选，消息锁的消息来源，可以为委托、[ListeningTarget](./ProcessingModel/ListeningTarget.md) 或 [MessageSource](../../Model/MessageSource.md)
    - timeout：可选，锁超时时间，超时后锁会被自动移除
    - onTimeout：可选，会在锁超时后被调用

<br>

## LockState
LockState 是表示锁处理状态的枚举，也是锁目标方法的返回值类型
- 字段
    - Finished：处理完成，移除锁
    - NotFinished：还未处理完成
    - Continue：继续将消息传递给下一个锁
    - ContinueAndRemove：继续将消息传递给下一个锁，并移除当前锁