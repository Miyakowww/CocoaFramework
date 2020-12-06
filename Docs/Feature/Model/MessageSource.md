# MessageSource

表示一个消息来源（群聊/私聊/临时）

<br>

## 属性
- IsGroup：是否为群聊消息
- IsTemp：是否为临时消息
- IsFriend：是否为朋友私聊消息
- IsOwner：是否为 Owner
- IsAdmin：是否为 Owner 或 Admin
- AuthLevel：用户的权限等级

<br>

## 字段
- group：QQ 群，可能为空
- user：QQ 用户

<br>

## 方法
- Send：向来源发送消息
- SendAsync：异步向来源发送消息
- SendEx：向来源发送消息，可设置如果为群时自动添加艾特
- SendExAsync：异步向来源发送消息，可设置如果为群时自动添加艾特
- SendReplyEx：向来源发送回复，需要提供回复的消息，可设置如果为群时自动添加艾特
- SendReplyExAsync：异步向来源发送回复，需要提供回复的消息，可设置如果为群时自动添加艾特
- SendPrivate：向来源的用户发送私聊消息，即使来源为群聊
- SendPrivateAsync：异步向来源的用户发送私聊消息，即使来源为群聊