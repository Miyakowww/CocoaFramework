# Cocoa Framework 介绍

## 基本概念
Cocoa Framework 提供了一个 QQ 消息处理模型，以及对这个模型的实现和支持。消息被接收后经由中间件处理，再传递给各个模块处理，最后将消息和处理结果传递至服务。
- [权限管理](./Basic/Permission.md)
- [数据管理](./Basic/Data.md)

<br>

## 启动与连接
- [BotStartup 和 BotStartupConfig](./Startup/BotStartup.md)

<br>

## 基本模型
- [QUser](./Model/QUser.md)
- [QGroup](./Model/QGroup.md)
- [QMessage](./Model/QMessage.md)
- [MessageSource](./Model/MessageSource.md)

<br>

## 框架构成
- [DisabledAttribute](./Framework/Disabled.md)
- [Middleware](./Framework/Middleware.md)
- [Module](./Framework/Module.md)
- [Service](./Framework/Service.md)
- [Component](./Framework/Component.md)
- [BotAPI](./Framework/BotAPI.md)
- [BotAuth](./Framework/BotAuth.md)
- [BotInfo](./Framework/BotInfo.md)
- [BotReg](./Framework/BotReg.md)
- [DataManager](./Framework/DataManager.md)