# 开始使用

要开始使用Lip你应该先完成[安装](installation.md)。如果你对命令行界面不熟悉，你也可以使用[LipUI](lipui_quickstart.md)来管理你的tooth。

## 确定你的Lip正常工作

第一步，你需要检查你的Lip的安装，执行下面的命令，并确保输出看起来相似。

```shell
> lip --version
Lip 0.11.0 from C:\Users\ExampleUser\AppData\Local\Lip\lip.exe
```

## 常见任务

### 安装一个tooth

```shell
> lip install github.com/liteldev/exampletooth@1.0.0
[...]
Successfully installed all tooth files.
```

默认情况下，Lip会通过GOPROXY，一个Git仓库的代理来获取tooth。

### 从URL安装tooth

```shell
> lip install https://example.com/exampletooth.tth
[...]
Successfully installed all tooth files.
```

Lip 只支持以`http://` 或 `https://`开头的URL，所有URL需要以`.tth`作为结尾。

### tooth文件安装tooth

```shell
> lip install exampletooth.tth
[...]
Successfully installed all tooth files.
```

tooth文件需要有`.tth`扩展名。

### 安装多个tooth

Lip 支持一次安装多个tooth

```shell
> lip install github.com/liteldev/exampletooth@1.0.0 github.com/liteldev/anotherexampletooth@1.0.0
[...]
Successfully installed all tooth files.
```

### 升级tooth

```shell
> lip install --upgrade github.com/liteldev/exampletooth
[...]
Successfully installed all tooth files.
```

### 卸载tooth

要卸载一个tooth，你必须提供该tooth的包路径。

```shell
> lip uninstall github.com/liteldev/exampletooth
[...]
Successfully uninstalled all tooths.
```

### 列出所有tooth

```shell
> lip list
Tooth                            Version
-------------------------------- ----------
github.com/liteldev/exampletooth 1.0.0
```

### 查看一个tooth的具体信息

```shell
> lip show github.com/liteldev/exampletooth
Tooth-path: github.com/liteldev/exampletooth
Version: 1.0.0
Name: Example Tooth
Description: An example tooth
Author: Example User
License: MIT
Homepage: www.example.com
```

## 下一步

你可以阅读 **命令** 目录下的页面，以获得Lip命令的更详细描述。