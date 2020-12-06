# 权限管理

## 身份
- Cocoa Framework 以 QQ 号作为身份标识，分为 Owner、Admin 和普通用户 3 类
- Owner 权限最高，仅能设置一个，一般设置为主开发者
- Admin 为管理员，可以设置多个，一般设置为其他贡献者
- 未设置为上述权限的用户均为普通用户
- 本框架仅开放了权限设置的端口（请参阅 [BotAuth](../Framework/BotAuth.md)），并未实现相关的管理功能

<br>

## 权限等级
- Owner 的等级为 2
- Admin 的等级为 1
- 普通用户的等级为 0