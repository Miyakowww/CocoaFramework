# Component

Component 是 Cocoa Framework 的自定义组件

<br>

## ComponentCore
管理 Component 的类
- 属性
    - Components：获取所有的 Component

<br>

## BotComponentAttribute 和 BotComponentBase
为类添加 BotComponentAttribute 并继承于 BotComponentBase 即可使类成为 Component。类名建议以大写 C 字母开头

<br>

## BotComponentBase.Init
此方法会在框架初始化时被调用，重写此方法可用于初始化