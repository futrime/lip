# 常见问题

## 我在哪里可以找到在线可用的tooth包？

你可以在[registry.litebds.com](https://registry.litebds.com)上找到它们。

## 我可以使用除 "registry.litebds.com "以外的注册表网站嘛？

当然！你可以使用任何你想要的注册表。只要将`LIP_REGISTRY`环境变量设置为你想使用的注册表，例如`LIP_REGISTRY=https://registry.litebds.com`。

## 它的下载速度太慢了! 我可以做什么呢？

Lip通过GOPROXY下载tooth。你可以将`LIP_GOPROXY`环境变量设置为一个用逗号隔开的GOPROXY服务器列表，例如`LIP_GOPROXY=https://goproxy.cn,https://goproxy.io`。设置一个离你很近的GOPROXY服务器。

## 当我试图安装一个tooth时，它总是显示错误!

可能是缓存被破坏了。尝试通过运行`lip cache purge`来清除缓存。

## 我该如何升级Lip？

对于全局安装，你必须下载最新的Lip版本，并替换旧的版本。对于本地安装，你可以运行`lip install --upgrade lip`来更新Lip。