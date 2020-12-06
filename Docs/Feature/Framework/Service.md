# Service

服务用于在消息处理的最后总结本次处理的结果。例如模块使用量统计就可以通过服务实现

<br>

## ServiceCore
管理 Service 的类
- 属性
    - Services：获取所有的 Service

<br>

## BotServiceAttribute 和 BotServiceBase
为类添加 BotServiceAttribute 并继承于 BotServiceBase 即可使类成为 Service。类名建议以大写 S 字母开头

<br>

## Init
此方法会在框架初始化时被调用，重载此方法可用于初始化

<br>

## Run
Service 需要重载此方法，用于总结本次处理的结果
- 参数
    - src：消息来源
    - msg：消息内容
    - origMsg：消息经过处理前的内容
    - processed：消息是否被模块处理
    - processModule：处理消息的模块，如果消息没被处理，本参数为 null