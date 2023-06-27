# 开发者快速入门

我们很高兴你对与Lip一起创作感兴趣。

## 确保你有一个正常工作的Lip

作为第一步，你应该检查你是否安装了一个工作的Lip。这可以通过运行以下命令来完成，并确保输出结果看起来类似。

```shell
> lip --version
Lip 0.1.0 from C:\Users\ExampleUser\AppData\Local\Lip\lip.exe
```

## 常用功能

### 初始化一个tooth工作区

```shell
> lip tooth init
What is the tooth path? (e.g. github.com/tooth-hub/llbds3)
github.com/tooth-hub/example
What is the name?
Example
What is the description?
An example tooth.
What is the author? Please input your GitHub username.
Bob
Successfully initialized a new tooth.
```

然后你可以在 tooth.json中填写信息，使你的作品被Lip认可。

### 打包tooth

目前我们还没有提供打包牙齿的命令。你可以直接压缩所有东西（确保 tooth.json 在压缩文件的根目录下），并将其扩展名从".zip "改为".th"。

## GOPROXY相关通知

由于我们使用GOPROXY作为代理来获取牙齿文件，请不要将go.mod文件放在你版本库的根目录下。

## 下一步行动

你可以阅读 [tooth.json 文件参考]( tooth_json_file_reference.md) 以了解更多信息。
