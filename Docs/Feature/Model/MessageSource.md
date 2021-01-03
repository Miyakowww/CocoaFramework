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
- group：QQ 群，如果来源为私聊，本字段为 null
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
- SendImage：向来源发送图片
- SendImageAsync：向来源异步发送图片
- SendVoice：向来源发送语音
- SendVoiceAsync：向来源异步发送语音
- Mute：禁言用户，仅在为群聊消息时可用
- MuteAsync：异步禁言用户，仅在为群聊消息时可用
- Unmute：取消禁言用户，仅在为群聊消息时可用
- UnmuteAsync：异步取消禁言用户，仅在为群聊消息时可用