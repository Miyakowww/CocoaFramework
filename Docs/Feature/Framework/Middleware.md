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

## Init
如果重载此方法，便会在框架初始化时被调用，可以用于初始化

<br>

## Run
Middleware 需要重载此方法，用于对消息进行预处理
- 参数
    - src：消息来源，对此参数的更改会直接影响到接下来的中间件/模块接收到的内容
    - msg：消息内容，对此参数的更改会直接影响到接下来的中间件/模块接收到的内容
- 返回值
    - 类型为 bool，表示是否允许消息继续传递