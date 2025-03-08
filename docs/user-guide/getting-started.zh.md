# 入门指南

要开始使用 lip，您需要在系统上[安装 lip](./installation.zh.md)。

## 检查 lip 是否正常工作

首先，打开终端并运行此命令以确保 lip 正确安装：


```bash
$ lip --version
0.25.1
```

如果看到类似于这样的输出，那么 lip 就已经正常工作了。

如果输出看起来不对，请查看[安装](./installation.zh.md)页面，那里有详细的步骤，帮助您在 Windows、macOS 或 Linux 上安装 lip。

## 常见使用方式

### 安装一个软件包

```bash
$ lip install example.com/pkg@1.0.0
```

默认情况下，lip 通过 [goproxy.io](https://goproxy.io) 获取软件包。如果您更喜欢直接使用 Git 获取软件包，只需清除代理列表即可：
  
```bash
lip config set go_module_proxies=
```

或者，设置一个自定义代理：

```bash
lip config set go_module_proxies=https://proxy.example.com
```

### 从本地目录安装一个软件包

```bash
$ lip install /path/to/pkg/
```

lip 将检测该目录中的 tooth.json 文件并安装软件包。

### 从本地压缩包安装一个软件包

```bash
$ lip install /path/to/pkg.tar.gz
```

### 使用 tooth.json 文件安装多个软件包

在当前目录中创建一个 tooth.json 文件，然后运行 `lip install` 来一次性安装多个软件包——一种非常方便的方法。

### 更新一个软件包

```bash
$ lip update example.com/pkg@1.0.0
```

请记住，更新软件包时始终指定版本。 lip 不支持自动更新到最新版本，因为软件包源没有完全同步。

### 卸载一个软件包

```bash
$ lip uninstall example.com/pkg
```

卸载软件包时，请不要指定版本。
