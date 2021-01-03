# BotInfo

提供好友和群聊关系相关信息的类

<br>

## 方法
- ReloadAll：重新加载全部信息，一般不需要手动调用
- ReloadAllGroupMembers：重新加载全部群成员列表，一般不需要手动调用
- ReloadGroupMembers：重新加载群成员列表，一般不需要手动调用
- ReloadFriends：重新加载好友信息，一般不需要手动调用
- HasGroup：机器人是否在某个群聊
- HasGroupMember：机器人是否在某个群并且该群包含某成员
- HasFriend：某用户是否为机器人的好友
- GetTempPath：获取发送临时消息所需的群信息