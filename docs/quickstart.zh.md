# 开始使用

要开始使用lip你应该先完成[安装](installation.md)。如果你对命令行界面不熟悉，你也可以使用[lipUI](lipui_quickstart.md)来管理你的tooth。

## 确定你的lip正常工作

第一步，你需要检查你的lip的安装，执行下面的命令，并确保输出看起来相似。

```shell
> lip --version
lip 0.15.0 from C:\Users\ExampleUser\AppData\Local\lip\lip.exe
```

## 常见任务

### 安装一个tooth

```shell
> lip install github.com/tooth-hub/bdsdownloader
[...]
Done.
```

默认情况下，lip会通过GOPROXY，一个Git仓库的代理来获取tooth。

### tooth文件安装tooth

```shell
> lip install ./bdsdownloader.tth
[...]
Done.
```

tooth文件需要有`.tth`扩展名。

### 安装多个tooth

lip 支持一次安装多个tooth

```shell
> lip install github.com/liteldev/bdsdownloader github.com/tooth-hub/crashlogger
[...]
Done.
```

### 升级tooth

```shell
> lip install --upgrade github.com/liteldev/bdsdownloader
[...]
Done.
```

### 卸载tooth

要卸载一个tooth，你必须提供该tooth的包路径。

```shell
> lip uninstall github.com/liteldev/bdsdownloader
[...]
Done.
```

### 列出所有tooth

```shell
> lip list
+------------------------------------+----------------+---------+
|               TOOTH                |      NAME      | VERSION |
+------------------------------------+----------------+---------+
| github.com/tooth-hub/bdsdownloader | BDS Donwloader | 0.3.1   |
| github.com/tooth-hub/crashlogger   | CrashLogger    | 1.0.1   |
| github.com/tooth-hub/peeditor      | PeEditor       | 3.2.0   |
+------------------------------------+----------------+---------+
```

### 查看一个tooth的具体信息

```shell
> lip show github.com/liteldev/exampletooth
+-------------+------------------------------------+
|     KEY     |               VALUE                |
+-------------+------------------------------------+
| Tooth Repo  | github.com/tooth-hub/bdsdownloader |
| Name        | BDS Donwloader                     |
| Description | A CLI tool to download BDS         |
| Author      | Jasonzyt                           |
| Version     | 0.3.1                              |
+-------------+------------------------------------+
```

## 下一步

你可以阅读 **命令** 目录下的页面，以获得lip命令的更详细描述。如果你想要创建一个tooth，你可以阅读[这个教程](tutorials/create_a_lip_tooth.md)。
