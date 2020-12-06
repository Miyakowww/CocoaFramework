# Module

模块用于处理消息

<br>

## ModuleCore
管理 Module 的类
- 属性
    - Modules：获取所有的 Module

- 方法
    - AddLock：添加 [MessageLock](./ModuleFeature/MessageLock.md)

<br>

## BotModuleAttribute
添加了本特性并继承于 BotModuleBase 的类会被 Cocoa Framework 视作 Module。类名建议以大写 M 字母开头
- 参数
    - name：模块名
    - level：所需的最低权限等级
    - privateAvailable：是否在私聊时可用
    - groupAvailable：是否在群聊中可用
    - showOnModuleList：可选，实现模块管理时可以以此为依据判断是否将此模块显示在模块列表中，默认为 true
    - processLevel：可选，将作为模块处理顺序的依据，数值越小，越先进行处理，默认为 0

<br>

## BotModuleData
Module 的固有数据
- 字段
    - ActiveGroup：启用本模块的群
    - BanUser：不被允许本模块的用户
    - LastStatistics：最后统计时间
    - Usage：使用量数据

<br>

## BotModuleBase
Module 的基类
- 属性
    - Enabled：模块是否被启用
    - ModuleData：模块数据

- 方法
    - SaveData：保存对当前模块数据的所有更改
    - SetGroup：设置群的许可状态
    - SetUser：设置用户的许可状态
    - AddUsage：添加一次模块使用
    - GetUsage：获取最近给定天数的使用次数
    - Init：此方法会在框架初始化时被调用，重载此方法可用于初始化
    - Run：此方法会在处理消息时被调用，返回值为 bool 类型，表示当前消息是否被本模块处理
    - GActive：给定群是否启用了本模块
    - UActive：给定用户是否被允许使用本模块
    - ActivityOverrode：GActive 方法是否被重载

<br>

## 其他特性
- [RegexRoute](./ModuleFeature/RegexRoute.md)
- [MessageLock](./ModuleFeature/MessageLock.md)
- [Meeting](./ModuleFeature/Meeting.md)
- [ModuleDataAttribute](./ModuleFeature/ModuleDataAttribute.md)