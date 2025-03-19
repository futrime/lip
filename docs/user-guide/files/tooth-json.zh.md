# tooth.json

`tooth.json`文件定义了包的元数据、标识符、版本、依赖项、文件和其他配置设置。此文件必须以JSON格式存在，并位于包的根目录中。

有关完整的JSON模式规范，请参见[tooth.v3.schema.json](../../schemas/tooth.v3.schema.json)。

在下面的文档中，字段被标记为（必需）或（可选）。请注意，如果父字段是可选的但包含必需的子字段，则只有当父字段被包含时，这些子字段才成为强制性的。例如，虽然
`variants`是可选的，但如果你包含它，每个变体都必须指定一个`platform`。

Scriban表达式允许你在任何字符串值中动态引用其他字段，使包维护更容易。例如，使用`{{tooth}}`引用包的tooth路径或`{{version}}`
访问包的版本号。这种动态引用有助于保持你的包配置DRY（不要重复自己）并更易于维护。然而，一些包清单操作命令（例如带有`--save`
选项的命令）可能不支持Scriban表达式。

## 字段

### format_version (必需)

格式版本号。目前仅支持版本`3`。

### format_uuid (必需)

格式的唯一标识符。目前仅支持`289f771f-2c9a-4d73-9f3f-8492495a924d`。

### tooth (必需)

包的tooth路径，Go模块路径格式中的唯一标识符（不带协议头和后缀的URL）。

示例:

- `github.com/LiteLDev/LeviLamina`

提示：要发布一个包，它的tooth路径必须与它所在仓库的URL匹配。

### version (必需)

The package version in [semantic versioning](https://semver.org) format.
该字段需要遵循[语义化版本](https://semver.org)格式。

示例:

- `1.0.0`
- `1.0.0-alpha.1`
- `0.1.0`

当创建Git标签时，版本号前需要加上`v`（例如`v1.0.0`）。然而，在`version`字段中需要省略`v`前缀。

不要使用`v0.0.0`，因为Go模块代理将其视为伪版本。

### info (可选)

包的元数据字段。

### info.name (可选)

包的显示名称。

### info.description (可选)

包的简要描述。

### info.tags (可选)

包标签采用以下格式之一：

- 简单格式: `tag`
- 键值对格式: `tag:subtag`

标签和子标签只能包含小写字母、数字和连字符（[a-z0-9-]
）。虽然lip平等对待这两种格式，但一些平台如[Bedrinth](https://bedrinth.com)可能使用它们的方式不同。

### info.avatar_url (可选)

包的头像/图标的URL。

### variants (可选)

由数组组成的平台特定包配置。这是`tooth.json`的核心配置部分。

lip按顺序处理变体，应用与当前平台匹配的所有变体。当多个变体匹配时，它们的配置将被合并。

注意：对于平台兼容性检查，lip会忽略使用glob模式的变体。要支持多个平台，即使它们为空，也必须为每个平台定义单独的变体。

### variants[].label (可选)

此变体的标签。用户可以使用`lip install <tooth>#<标签>@<版本>`通过标签安装特定变体。应与`^[a-z0-9]+(_[a-z0-9]+)*$`
匹配或是一个glob模式。如果省略或为空，则该变体被视为默认值。

为了使变体标签被识别，在变体数组中必须至少有一个带有非glob标签的变体。标签字段中的glob模式只有在它们对应的非glob标签也被定义时才会生效。例如：

- 只有`label_*`并不表示支持任何标签
- 要支持`label_a`和`label_b`，你需要：
    - 两个带有确切标签的独立变体
    - 一个带有确切标签的变体和另一个带有`label_*`的变体

### variants[].platform (可选)

此变体的目标平台。有效值：

- `linux-arm64`
- `linux-x64`
- `osx-arm64`
- `osx-x64`
- `win-arm64`
- `win-x64`
- Glob 语法 (e.g., `linux-*`)

为了使平台变体被识别，在变体数组中必须至少有一个非glob平台变体。平台字段中的glob模式只有在它们对应的非glob平台也被定义时才会生效。例如：

- 只有`linux-*`并不表示支持任何Linux平台
- 要支持`linux-x64`和`linux-arm64`，你需要：
    - 两个带有确切平台的独立变体
    - 一个带有确切平台的变体和另一个带有`linux-*`的变体

如果省略或为空，则该变体被视为平台无关，即始终为当前平台。

### variants[].dependencies (可选)

软件包的依赖，指定为键值对。键是包标识符（可选带有子目录路径），值是版本约束。

示例:

键:

- `github.com/futrime/example-package`
- `github.com/futrime/example-package#subpath`

版本约束:

- 确切版本: `1.0.0`
- 范围: `>=0.1.0 <1.0.0`

版本解析使用[WalkerCodeRanger/semver](https://github.com/WalkerCodeRanger/semver)。

### variants[].assets (可选)

定义应如何处理软件包中的文件。

!!! warning
    因为lip会将非uncompressed类型的assets读取进内存以提升性能，所以不要引入过大的非uncompressed类型的assets，如果你的assets过大，可以考虑使用外部工具（例如7-zip）来解压assets。

### variants[].assets[].type (必需)

资产类型:

- `self`: 文件从包本身获取
- `tar`: TAR 归档包
- `tgz`: Gzip格式压缩的 TAR 归档包
- `uncompressed`: 单个未压缩的文件
- `zip`: ZIP 压缩包

### variants[].assets[].urls (可选)

该资产的下载URL，按顺序尝试。对于`self`类型的资产，该数组需要为空。（是的这是个数组）

### variants[].assets[].placements (可选)

在工作区中放置文件的规则。接受一个放置规则数组。

### variants[].assets[].placements[].type (必需)

放置类型:

- `file`: 放置单个文件
- `dir`: 放置目录

提示: `uncompressed` 资产仅支持 `file` 类型放置.

### variants[].assets[].placements[].src (必需)

源路径规范：

- 对于`uncompressed`资产: 值为空 (`""`)
- 对于`file`类型: 文件路径或者glob表达式 （如果匹配到目录将会被被忽略，文件将会被展平）
- 对于`dir`类型: 目录路径（会保留结构）

### variants[].assets[].placements[].dest (必需)

目标路径:

- 对于文件放置: 目标是文件的路径
- 对于目录/glob放置: 目标是文件夹的路径

### variants[].preserve_files (可选)

在卸载过程中保留的文件路径或glob模式数组。

### variants[].remove_files (可选)

在卸载过程中要删除的文件路径或glob模式数组。这将覆盖`preserve_files`设置。

### variants[].scripts (可选)

在工作区中要执行的命令。定义为一个键值对，其中键是脚本名称，值是要按顺序执行的命令数组。如果多个匹配的变体定义了相同的脚本，即使脚本未定义，也只会使用最后一个定义的脚本，即建议尽可能晚地定义脚本。

内置脚本钩子（所有值都是字符串数组）：

- `pre_install`: 安装前
- `install`: 文件放置后
- `post_install`: 安装后
- `pre_pack`: 包装前（仅适用于默认变体）
- `post_pack`: 包装后（仅适用于默认变体）
- `pre_uninstall`: 卸载前
- `uninstall`: 文件删除后
- `post_uninstall`: 卸载后

可以使用`lip run <script>`运行自定义脚本。自定义脚本名称应匹配`^[a-z0-9]+(_[a-z0-9]+)*$`，并且也期望值是命令数组（
`pre_install`之类的）。
