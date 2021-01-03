# QMessage

表示一条 QQ 消息

<br>

## 属性
- ID：消息 ID
- Time：消息时间
- PlainText：消息中的纯文本，便于正则匹配（请参阅 [RegexRoute](../Framework/ModuleFeature/RegexRoute.md)）。其中表情会被转换成/fxxx格式

<br>

## 字段
- chain：消息链

<br>

## 方法
- GetSubMessages：获取消息链中特定类型的消息对象，以数组形式返回