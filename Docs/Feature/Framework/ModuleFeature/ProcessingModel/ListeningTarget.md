# ListeningTarget

监听目标，可用作 MessageLock 和 Meeting 的目标

<br>

## 字段
- group：监听的群，如果为空表示监听全部群
- user：监听的用户，如果为空表示监听全部用户

<br>

## 方法
- Fit：目标是否符合本 ListeningTarget
- FromGroup：从群聊创建 ListeningTarget
- FromUser：从用户创建 ListeningTarget
- FromTarget：从来源创建 ListeningTarget