# 创建一个lip齿包

这是一个教程的第一部分，介绍了lip的一些基本特性。如果你刚开始使用lip，一定要看看[入门指南](../quickstart.md)，它介绍了lip的基本命令。在这个教程中，你将创建一个包含LeviLamina插件的齿包。

## 先决条件

- **一些项目管理经验。**你应该提前学习Git的基本用法，以及JSON的基本语法。

- **一个用来编辑tooth.json的工具** 你有的任何文本编辑器都可以。最流行的是VSCode和Vim。

- **一个命令终端** lip在Windows的PowerShell和cmd中都可以很好地工作。

- **lip命令行工具** 你应该提前安装lip。更多信息，请参考[安装](../installation.md)

## 准备插件分发

lip在安装时会获取Git仓库的一个版本的所有内容。因此，你应该准备好所有要安装的文件，并放在Git的管理下。

如果你只是使用文本工作（例如脚本插件，附加组件），你可以直接使用开发的仓库来创建一个齿包。但是，如果你使用二进制文件（例如本地插件，世界），你可能需要创建另一个仓库来存储内容。

否则，二进制文件可能会使你的仓库过大，难以管理。

在这个例子中，我们假设仓库的结构如下：

```text
exampleplugin.dll
exampleplugin/
  config.json
  libexample.dll
  data/
```

## 初始化齿包

打开一个命令提示符，然后cd到仓库的根目录。如果你使用Windows，你可以在文件资源管理器中按*shift*并右键单击，然后单击“在此处打开PowerShell窗口”。

运行下面的命令来初始化齿包。该命令将在仓库的根目录下创建一个tooth.json文件。

```shell
lip tooth init
```

编辑tooth.json。填写尖括号（“<”和“>”）中的内容。

- tooth字段表示齿包的路径。如果你想发布齿包，它必须是没有协议前缀（例如https://或http://）的齿包仓库URL。

- placement字段表示lip将如何从齿包复制文件到BDS。源路径基于齿包的根目录（或在这个例子和大多数情况下的仓库），而目标路径基于BDS的根目录，其中“bedrock_server.exe”位于。

- possession字段表示这个齿包的私有目录。它在卸载齿包时会被删除，但在重新安装或升级齿包时会保留。注意，possession字段中指示的路径基于BDS的根目录。每个项目都应以“/”结尾。

## 测试齿包

在发布齿包之前，你应该测试它，以确保它按预期工作。

将仓库根目录下的所有文件压缩，并将zip文件重命名为“exampleplugin.tth”。

将zip文件复制到某个目录，然后运行下面的命令来安装齿包。

```shell
lip install exampleplugin.tth
```

运行下面的命令来卸载齿包。

```shell
lip uninstall exampleplugin.tth
```

运行下面的命令来再次安装齿包。

```shell
lip install exampleplugin.tth
```

检查齿包是否按预期工作。

## 发布你的齿包

- 存储并提交更改，然后将它们推送到公共Git服务。

- 添加一个标签，并使用版本名称发布一个发布。标签名称应该是版本名称加上前缀“v”，例如“v1.0.0”。

## 另一个示例：将Minecraft世界制作成一个齿包

通常，Minecraft世界具有以下文件结构：

```text
Bedrock level/
  level.dat
  level.dat_old
  levelname.txt
  db/
```

您可以创建一个名为`tooth.json`的文件，内容如下：

```json
{
    "format_version": 2,
    "tooth": "example.com/exampleuser/exampleworld",
    "version": "1.0.0",
    "dependencies": {},
    "information": {
        "name": "Example World",
        "description": "An example world",
        "author": "Example User",
        "tags": [
            "ll", "llbds", "bds"
        ]
    },
    "files": {
        "place": [
            {
                "src": "Bedrock level/*",
                "dest": "worlds/Bedrock level/"
            }
        ]
    }
}
```

## 下一步

你可以阅读[tooth.json文件参考](../reference/tooth_json_file_reference.md)以获得更多参考。
