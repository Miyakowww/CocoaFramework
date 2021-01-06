# Middleware

中间件用于对消息进行预处理，拥有截断消息和更改消息来源、消息内容的权限。例如黑名单就可以通过中间件实现

<br>

## MiddlewareCore
管理 Middleware 的类
- 属性
    - Middlewares：获取所有的 Middleware

<br>

## BotMiddlewareBase
Middleware 需继承于 BotMiddlewareBase，且需要在启动前添加进 BotStartupConfig.Middlewares。类名建议以大写 W 字母开头

<br>

## BotMiddlewareBase.Init
此方法会在框架初始化时被调用，重写此方法可用于初始化

<br>

## BotMiddlewareBase.OnMessage
重写此方法以对消息进行预处理
- 参数
    - src：消息来源，对此参数的更改会直接影响到接下来的中间件/模块接收到的内容
    - msg：消息内容，对此参数的更改会直接影响到接下来的中间件/模块接收到的内容
- 返回值
    - 类型为 bool，表示是否允许消息继续传递
- 线程安全性
    - 此方法会以线程安全的方式调用。如果您确定重写方法是线程安全的，也可以添加 ThreadSafe 特性使框架以更高效的方式调用