# BotAuth

提供权限管理相关功能的类

<br>

## 属性
- Owner：Owner 的 QQ 号
- Admin：Admin 的 QQ 号列表
- HasOwner：是否设置了 Owner

<br>

## 方法
- IsOwner：判断给定的 QQ 是否为 Owner
- IsAdmin：判断给定的 QQ 是否为 Owner 或 Admin
- AuthLevel：获取给定的 QQ 的权限等级
- SetOwner：设置 Owner
- SetAdmin：添加 Admin
- RemoveAdmin：移除 Admin
- RemoveAdminAt：根据下标移除 Admin