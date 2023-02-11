# 创作者指南

我们很高兴你对使用Lip创作感兴趣。


## 确定你的Lip正常工作

第一步，你需要检查你的Lip的安装，执行下面的命令，并确保输出看起来相似。

```shell
> lip --version
Lip 0.1.0 from C:\Users\ExampleUser\AppData\Local\Lip\lip.exe
```

## 常见任务

### 初始化齿包工作区

```shell
> lip tooth init
tooth.json created successfully
please edit tooth.json and modify the values with "<>"
```

然后你可以在 tooth.json中填写信息，使你的作品被Lip认可。

### 打包齿包

目前，我们没有提供打包齿包的命令，你可以直接压缩所有的东西 (确保 tooth.json 在你的压缩文件的根目录) 并将拓展名从".zip" 改为 ".tth" 。


## GOPROXY相关通知

由于我们使用GOPROXY作为代理来获取齿包文件，请不要将go.mod文件放在你版本库的根目录下。

## 下一步

你可以阅读 [tooth.json 文件参考](tooth_json_file_reference.md) 来获取更多信息