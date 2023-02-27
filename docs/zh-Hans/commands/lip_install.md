# lip install

## 用法

```shell
lip install [options] <requirement specifiers>
lip install [options] <tooth url/files>
```

## 功能

从下列途径安装安装tooth：  

- Goproxy上的的tooth存储库
- 本地或远程的独立tooth文件（后缀为`.tth`）。

对于tooth的存储库，你可以通过添加后缀来指定版本，如`@1.2.3`或`@1.2.0-beta.3`。然而，当安装了另一个版本，而你运行Lip时没有`--upgrade`或`--force-reinstall`标志，Lip将不会安装特定的版本。

只有小写字母、数字、破折号、下划线、圆点、斜线[a-z0-9-_./]和一个@在需求说明中被允许。

如果你设置了环境变量GOPROXY，Lip将通过它来访问tooth存储库。否则，Lip将选择默认的Goproxy <https://goproxy.io>.

### 概述

`lip install` 有几以下个阶段：

1. 确定基本要求。用户提供的参数在这里处理。
2. 获取tooth并处理依赖关系。一旦获取到对应的tooth，依赖关系就会被解决。
3. 安装tooth。 （并卸载任何正在升级的东西）

注意 `lip install` Lip更倾向于让安装的版本保持原样，除非使用 `--upgrade` 标记。

### 参数处理

在查看要安装的项目时，Lip按以下步骤检查每个项目是什么类型的：

1. 后缀为`.tth` ，前缀为 `http://` 或 `https://` 的远程牙文件。
2. 本地tooth文件，后缀为`.tth`。
3. tooth库，可以通过Goproxy访问。
4. tooth别名，可以在Lip注册表中查找。

在3和4中，所有字母在处理前将被转换为小写。

### Lip注册表

从v0.8.0开始，Lip支持Lip注册表，这使得你可以使用别名来安装tooth。默认情况下，Lip会使用<https://registry.litebds.com>上的注册表。你也可以通过设置环境变量`LIP_REGISTRY`到你的注册表的URL来使用你自己的注册表。

### 安装依赖

一旦Lip有了要安装依赖的包，它就会选择每个包需求来决定要安装的依赖的版本，未提供更多依赖要求时，将会安装最新的稳定版作为依赖。

### 依赖关系

Lip在安装依赖之前，是按照 "拓扑顺序 "安装依赖。当遇到依赖关系图中的循环时，Lip会拒绝安装该tooth。所有的开发者都应该避免依赖关系图中的任何循环。

这个依赖关系图将由Lip维护。当卸载某些软件包时，Lip会检查该图以确保所有存在依赖关系的包都卸载了。如果没有，Lip会问你是否要卸载它们或取消该操作。

### 预发布版本

你可以通过指定版本来安装任何预发布的版本。而tooth可以声明预发布版本作为他们的依赖项。然而，当tooth使用任何类型的范围版本匹配或通配符时，Lip 将忽略预发布的版本。

## 选项

- `-h, --help`

  展示帮助。

- `--upgrade`

  将指定的tooth升级到最新的可用版本。如果指定了一个版本并且它是较新的，则升级到该版本。对依赖关系的处理取决于所使用的升级策略。升级时，Lip将首先卸载旧版本，然后安装新版本。

- `--force-reinstall`

  重新安装tooth，即使它们已经是最新的了。重新安装时，Lip会先卸载已安装的tooth，然后再安装它。如果指定了版本，Lip将安装该版本，否则就是最新的版本。

- `-y, --yes`

  对所有的提示都以肯定回答，并以非交互式方式运行。

- `--numeric-progress`

  显示数字进度而不是进度条。

- `--no-dependencies`

  不安装依赖

## 样例

从tooth存储库安装。

```shell
lip install example.com/some_user/some_tooth         # Latest version
lip install example.com/some_user/some_tooth@1.0.0   # Specific version
lip install github.com/LiteLDev/LiteLoaderBDS@2.11.0 # LiteLoderBDS 2.11.0
```


升级一个已经安装的tooth。

```shell
lip install --upgrade example.com/some_user/some_tooth
```

强行重新安装一个tooth。

```shell
lip install --force-reinstall example.com/some_user/some_tooth
```

从tooth的URL安装。

```shell
lip install https://example.com/example.tth
```

从本地牙安装：

```shell
lip install example.tth
lip install ./example/example.tth
```

用一个别名来安装：

```shell
lip install liteloaderbds
```