## 本教程将借助 MiraiOK 以 Windows 平台为例讲解如何配置 Mirai
<br>

### 你需要
- [MiraiOK](https://github.com/LXY1226/MiraiOK) 的 amd64 版本（[下载地址](http://t.imlxy.net:64724/mirai/MiraiOK/miraiOK_windows-amd64.exe)）
- [mirai-api-http](https://github.com/project-mirai/mirai-api-http)（[Release 地址](https://github.com/project-mirai/mirai-api-http/releases)，下载最新版的 jar 文件就行）

<br>

### 准备 Mirai
1. 创建一个单独的文件夹，建议以英文命名
1. 将 miraiOK_windows-amd64.exe 放进文件夹，双击运行。此时会出现一些红色的报错信息，不用在意，等待程序自动退出
1. 再次打开 MiraiOK，最后出现 "mirai-console started successfully." 表示初始化成功

<br>

### 准备 mirai-api-http
1. 上一步正常情况下会在文件夹中生成许多新的文件和文件夹。关闭 MiraiOK 后将 mirai-api-http 的 jar 文件放进新生成的 plugins 文件夹内，再次开启 MiraiOK。
1. 加载完成后关闭 MiraiOK

<br>

### 配置自动登录
1. 打开 config 文件夹内的 Console 文件夹里的 AutoLogin.yml 文件，如果没有相关的编辑器直接用记事本就行
1. 将第二行的 123456654321 换成机器人的 QQ 号，example 换成密码
1. 保存

<br>

### 配置 mirai-api-http
1. 打开 config 文件夹内的 MiraiApiHttp 文件夹里的 setting.yml 文件，如果没有相关的编辑器直接用记事本就行
1. 将 host: 后面的 0.0.0.0 改为 127.0.0.1（如果有需要可以更改为别的内容）
1. 任选一个 10000 到 65535 之间的数字作为端口号，替换 port: 后面的 8080
1. 想一串密码作为钥匙，替换 authKey: 后面的 INITKEY……
1. 保存

<br>

### 完成
1. 开启 MiraiOK，可能需要进行设备锁认证相关操作，跟随提示进行即可
2. 至此，Mirai 配置完毕

<br>

### 备注
1. 127.0.0.1 表示当前主机，也就是你的电脑。如果需要让 Mirai 运行在服务器上并进行远程连接，host 应配置为服务器的公网 IP
2. 许多软件和协议限制了使用的端口号，如 http 使用 80 端口、MySQL 使用 3306 端口、MC 服务器使用 25565 端口等。请避免使用这些端口。同时 8080 端口常被用作默认端口，可以看到 mirai-api-http 也不例外，所以应避免使用此端口（WWW 代理服务除外）