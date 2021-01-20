# TextRoute

文本路由是 OnMessage 方法之外对消息的另一种处理方式，对于大量简单的需求可以极大地节省代码量，也可以使逻辑更加清晰

<br>

## TextRouteAttribute
Module 中添加了此特性的方法将被视为一个入口，收到符合条件的消息时本方法会被调用
- 参数
    - text：文本
    - IgnoreCase：可选，表示是否忽略大小写

<br>

## 入口方法
- 参数
    - 参数可以任意填写，除下述情况的参数都将被传入默认值或 null
    - 第一个参数类型为 [MessageSource](../../Model/MessageSource.md) 的参数将被传入消息的来源
    - 第一个参数类型为 [QMessage](../../Model/QMessage.md) 的参数将被传入消息的内容
    
- 返回值
    - 入口方法的返回值可以是任意类型
    - 如果为 void 表示一旦被调用就代表消息被处理
    - 如果为 bool 类型表示消息是否被处理
    - 如果为 string 或 StringBuilder 类型且不为空将自动向来源发送对应文本，否则表示消息未被处理
    - 如果为 IEnumerator 或 IEnumerable 会被自动添加为 [Meeting](./Meeting.md)
    - 如果为其他值类型，返回结果不为默认值将代表消息被处理
    - 如果为其他引用类型，返回结果不为 null 将代表消息被处理

<br>

## 线程安全性
TextRoute 会以线程安全的方式调用。如果您确定入口方法是线程安全的，也可以添加 ThreadSafe 特性使框架以更高效的方式调用