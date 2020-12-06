# BotStartup 和 BotStartupConfig

## BotStartup
- BotStartup 提供 Start 和 Dispose 方法
- Start 方法用于连接和初始化框架，异步，返回 bool 表示连接成功与否
```CSharp
public static async Task<bool> Start(BotStartupConfig config)
```
- Dispose 方法用于释放资源，请注意，无论是否连接，连接成功与否，都需要在退出程序前调用此方法释放资源
```CSharp
public static async ValueTask Dispose()
```

<br>

## BotStartupConfig
- BotStartupConfig 用于提供 BotStartup.Start() 所需的信息
- 创建实例时需提供 host, port, authKey 和机器人 QQ
```CSharp
public BotStartupConfig(string host, int port, string authKey, long qqID)
```
- Middlewares 属性用于添加中间件，其中各中间件的排序决定最终中间件的执行顺序
- assembly 用于标记用户所使用的程序集，Cocoa Framework 将从此程序集中搜索各个模块，默认会设置为程序入口所在的程序集。如果模块类所在程序集不是入口程序集，请自行设置此字段。