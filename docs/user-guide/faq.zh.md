# 常见问题

## 我该怎么清除缓存?

```bash
$ lip cache clean
```

## 我该怎么更新/卸载 lip?

目前，lip 安装程序尚未实现。你可以手动更新/卸载 lip，只需用最新版本替换/删除 PATH 中的二进制文件即可。如果你正在卸载 lip，也可以删除 PATH 配置。


## 如何创建一个包

首先，运行以下命令将当前目录初始化为包：

```bash
$ lip init
```

然后，编辑 `tooth.json` 文件以定义包。有关更多信息，请参阅 [tooth.json 参考](./files/tooth-json.zh.md)。


要测试包的安装（请注意当前目录中的现有文件可能会受到影响，因为包是在当前目录中安装的），运行：

```bash
$ lip install
```

要打包该tooth包，运行：

```bash
$ lip pack my-package.zip
```
