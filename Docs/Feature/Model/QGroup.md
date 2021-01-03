# QGroup

表示一个 QQ 群

<br>

## 属性
- ID：QQ 群号

<br>

## 方法
- SendMessage：发送消息
- SendMessageAsync：异步发送消息
- SendImage：发送图片
- SendImageAsync：异步发送图片
- SendVoice：发送语音
- SendVoiceAsync：异步发送语音
- MuteMember：禁言群成员，仅在为群聊消息时可用
- MuteMemberAsync：异步禁言群成员，仅在为群聊消息时可用
- UnmuteMember：取消禁言群成员，仅在为群聊消息时可用
- UnmuteMemberAsync：异步取消禁言群成员，仅在为群聊消息时可用
- GetMemberCard：获取群成员卡片
- GetMemberCardAsync：异步获取群成员卡片
- MuteGroup：开启全体禁言
- MuteGroupAsync：异步开启全体禁言
- UnmuteGroup：关闭全体禁言
- UnmuteGroupAsync：异步关闭全体禁言
- KickMember：将群成员踢出群聊
- KickMemberAsync：异步将群成员踢出群聊
- LeaveGroup：退出群聊
- LeaveGroupAsync：异步退出群聊