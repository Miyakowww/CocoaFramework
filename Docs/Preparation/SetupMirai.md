## 本教程将讲解如何配置 Mirai

<br>

#### 如果从未使用过 Mirai，请先尝试 [启动 Mirai Console](https://github.com/mamoe/mirai-console/blob/master/docs/Run.md)

<br>

### 你需要
- [mirai-api-http](https://github.com/project-mirai/mirai-api-http)（[Release 地址](https://github.com/project-mirai/mirai-api-http/releases)，下载最新版的 jar 文件即可）

<br>

### 准备 mirai-api-http
1. 将 mirai-api-http 的 jar 文件放进 plugins 文件夹内，启动 Mirai
1. 加载完成后关闭 Mirai

<br>

### 配置 mirai-api-http
1. 打开 config 文件夹内的 MiraiApiHttp 文件夹里的 setting.yml 文件
1. 任选一个 10000 到 65535 之间的数字作为端口号，替换 port: 后面的 8080
1. 想一串密码作为钥匙，替换 authKey: 后面的 INITKEY……
1. 保存
1. 启动 Mirai，至此，Mirai 配置完毕

<br>

### 备注
1. 许多软件和协议限制了使用的端口号，如 http 使用 80 端口、MySQL 使用 3306 端口、MC 服务器使用 25565 端口等。请避免使用这些端口。同时 8080 端口常被用作默认端口，可以看到 mirai-api-http 也不例外，所以大多数情况下应避免使用此端口。常用端口主要集中于 10000 以下，故上文推荐新手选择 10000 以上的端口
1. 如使用 Windows 操作系统，请避免使用记事本。否则可能因为编码问题导致配置读取失败。出现如不会自动登录等问题