# tooth.json文件参考

每个tooth都由一个tooth.json文件定义，该文件描述了tooth的属性，包括其对其他tooth的依赖关系和其他信息。

您可以通过运行lip tooth init命令来生成一个tooth.json文件。下面的例子创建了一个tooth.json文件：

```shell
lip tooth init
```

## 模式

请参考<https://github.com/lippkg/lip/blob/main/schemas/tooth.v2.schema.json>。

## 示例

一个tooth.json包含如下所示的指令。这些指令在本主题的其他地方有描述。

```json
{
  "format_version": 2,
  "tooth": "github.com/tooth-hub/example",
  "version": "1.0.0",
  "info": {
    "name": "Example",
    "description": "An example package",
    "author": "exmaple",
    "tags": [
      "example"
    ],
    "avatar_url": "avatar.png"
  },
  "asset_url": "https://github.com/tooth-hub/example/releases/download/v1.0.0/example-1.0.0.zip",
  "commands": {
    "pre_install": [
      "echo \"pre_install\""
    ],
    "post_install": [
      "echo \"post_install\""
    ],
    "pre_uninstall": [
      "echo \"pre_uninstall\""
    ],
    "post_uninstall": [
      "echo \"post_uninstall\""
    ]
  },
  "dependencies": {
    "github.com/tooth-hub/example-deps": ">=1.0.0 <2.0.0 || >=3.0.0 <4.0.0"
  },
  "prerequisites": {
    "github.com/tooth-hub/example-pre": ">=1.0.0"
  },
  "files": {
    "place": [
      {
        "src": "example.js",
        "dest": "dir/example.js"
      },
      {
        "src": "plug/*",
        "dest": "dir/plug/"
      }
    ],
    "preserve": [
      "dir/data.json"
    ],
    "remove": [
      "dir/temp.txt"
    ]
  }
}
```

## `format_version`（必需）

表示tooth.json文件的格式。lip会根据这个字段来解析tooth.json文件。

### 示例

```json
{
  "format_version": 2
}
```

### 注意

您应该将format_version设置为2。

## `tooth`（必需）

声明tooth的tooth仓库路径，这是tooth的唯一标识符（结合tooth版本号）。

### 语法

通常，tooth路径应该是没有协议前缀的URL的形式（例如github.com/tooth-hub/corepack）。

只允许使用字母、数字、破折号、下划线、点和斜杠[A-Za-z0-9-_./]。必须与tooth仓库路径相同。

### 示例

```json
{
  "tooth": "github.com/tooth-hub/mytooth"
}
```

### 注意

tooth路径必须唯一地标识您的tooth。对于大多数tooth，路径是一个URL，lip可以在那里找到代码。对于不会直接下载的tooth，tooth路径可以是您控制的一些能确保唯一性的名称。

请注意，tooth路径不应该包含协议前缀（例如"https://"或"git://"），这已经违反了语法。

如果您想发布您的tooth，请将tooth路径设置为一个真正的URL。例如，第一个字符应该是一个字母或数字。

## `version`（必需）

### 语法

我们采用了[语义化版本2.0.0](https://semver.org)。

### 示例

生产发布的例子：

```json
{
  "version": "1.2.3"
}
```

预发布的例子：

```json
{
  "version": "1.2.0-beta.3"
}
```

早期开发发布的例子：

```json
{
  "version": "0.1.2"
}
```

### 注意

当发布您的tooth时，您应该用前缀"v"来设置Git标签，例如v1.2.3。否则，lip将无法正确解析标签。

由于GOPROXY将前缀为"v0.0.0"的版本视为伪版本，如果您想发布您的tooth，您不应该将版本设置为以"0.0.0"开头的。

## `info`（必需）

声明您的tooth的必要信息。

### 语法

以JSON对象的形式提供有关您的tooth的信息，包含以下字段：

- `name`：（必需）您的tooth的名称。
- `description`：（必需）您的tooth的简短描述。
- `author`：（必需）您的tooth的作者。
- `tags`：（必需）您的tooth的标签数组。只允许使用[a-z0-9-]。
- `avatar_url`：tooth的头像的URL。如果没有设置，将使用默认头像。如果提供了相对路径，它将被视为相对于**源仓库路径**的路径。

!!!tip
    tags不应该包含大写字母

### 示例

```json
{
  "info": {
    "name": "Example",
    "description": "An example package",
    "author": "example",
    "tags": [
      "example"
    ],
    "avartar_url": ""
  }
}
```

### 注意

有些标签有特殊的含义：

平台：

- `bds`：表示tooth应该安装在Minecraft Bedrock Dedicated Server平台上。
- `levilamina`：表示tooth应该安装在LeviLamina平台上。
- `lse`：表示tooth应该安装在LegacyScriptEngine平台上。
- `pnx`：表示tooth应该安装在PowerNukkitX平台上。

类型：

- `addon`：表示tooth是一个附加组件。
- `library`：表示tooth是一个库。
- `plugin`：表示tooth是一个插件。
- `plugin-engine`：表示tooth是一个插件引擎。
- `utility`：表示tooth是一个实用工具。
- `world`：表示tooth是一个世界。

这些标签将用于在搜索时过滤tooth。

## `asset_url`（可选）

声明tooth资产的URL。如果设置了这个字段，lip将下载资产并使用资产归档中的文件，而不是tooth仓库中的文件。这有助于发布大的二进制文件。

### 语法

URL应该是指向资产文件的直接链接。资产文件应该是一个zip归档文件。

### 示例

```json
{
  "asset_url": "https://github.com/tooth-hub/example/releases/download/v1.0.0/example-1.0.0.zip"
}
```

### 注意

对于GitHub链接，将使用配置的GitHub镜像来下载资产。如果没有配置镜像，将使用GitHub官方地址。

## `commands`（可选）

声明在安装或卸载tooth之前或之后运行的命令。

### 语法

这个字段包含四个子字段：

- `pre-install`：一个在安装tooth之前运行的命令的数组。（可选）
- `post-install`：一个在安装tooth之后运行的命令的数组。（可选）
- `pre-uninstall`：一个在卸载tooth之前运行的命令的数组。（可选）
- `post-uninstall`：一个在卸载tooth之后运行的命令的数组。（可选）

数组中的每一项都是一个要运行的命令的字符串。命令将在工作空间中运行。

### 示例

```json
{
  "commands": {
    "pre-install": [
      "echo Pre-install command"
    ],
    "post-install": [
      "echo Post-install command"
    ],
    "pre-uninstall": [
      "echo Pre-uninstall command"
    ],
    "post-uninstall": [
      "echo Post-uninstall command"
    ]
  }
}
```

## `dependencies`（可选）

声明您的 tooth 的依赖项。

### 语法

有关版本范围的语法，请参阅[此处](https://github.com/blang/semver#ranges)。

### 示例

```json
{
    "dependencies": {
        "github.com/tooth-hub/example-deps": ">=1.0.0 <=1.1.0 || 2.0.x"
    }
}
```

## `prerequisites`（可选）

声明您的 tooth 的先决条件。语法与 `dependencies` 字段相同，但先决条件不会被 lip 自动安装。

### 注意

某些 tooth 不应自动安装，例如 bds。自动安装这些 tooth 可能会导致严重的不兼容性问题。

## `files`（可选）

描述如何处理 tooth 中的文件。

### 语法

此字段包含三个子字段：

- `place`：一个数组，用于指定 tooth 中的文件应该放置到工作区的方式。每个项目都是一个对象，具有三个子字段：（可选）
  - `src`：文件的源路径。它可以是文件或带有后缀“*”的目录（例如 `plug/*`）。 （必需）
  - `dest`：文件的目标路径。它可以是文件或目录。如果 `src` 有后缀“*”，则 `dest` 必须是目录。否则，`dest` 必须是文件。 （必需）
- `preserve`：一个数组，用于指定在卸载 tooth 时应保留 `place` 字段中的哪些文件。每个项目都是文件路径的字符串。 （可选）
- `remove`：一个数组，用于指定在卸载 tooth 时应删除哪些文件。每个项目都是文件路径的字符串。 （可选）

### 示例

```json
{
    "files": {
        "place": [
            {
                "src": "plug/*",
                "dest": "plugins"
            },
            {
                "src": "config.yml",
                "dest": "config.yml"
            }
        ],
        "preserve": [
            "config.yml"
        ],
        "remove": [
            "plugins/ExamplePlugin.dll"
        ]
    }
}
```

### 注意

- 在 `place` 中指定但不在 `preserve` 中的文件将在卸载 tooth 时被删除。因此，您无需在 `remove` 中指定它们。
- `remove` 字段优先于 `preserve` 字段。如果一个文件在两个字段中都有指定，它将被删除。
- 只有 `place` 字段支持“*”后缀。`preserve` 和 `remove` 字段不支持它。

## `platforms`（可选）

声明特定于平台的配置。

### 语法

此字段是一个特定于平台的配置数组。每个项目都是一个带有以下子字段的对象：

- `asset_url`：与`asset_url`字段相同。（可选）
- `commands`：与`commands`字段相同。（可选）
- `dependencies`：与`dependencies`字段相同。（可选）
- `prerequisites`：与`prerequisites`字段相同。（可选）
- `files`：与`files`字段相同。（可选）
- `goos`：目标操作系统。有关值，请参见[此处](https://go.dev/doc/install/source#environment)。（必填）
- `goarch`：目标架构。有关值，请参见[此处](https://go.dev/doc/install/source#environment)。省略表示匹配所有。（可选）

如果提供并匹配，特定于平台的配置将覆盖全局配置。

### 示例

```json
{
    "platforms": [
        {
            "commands": {
                "pre-install": [
                    "echo Pre-install command for Windows"
                ]
            },
            "dependencies": {
                "github.com/tooth-hub/example-deps": ">=1.0.0 <=1.1.0 || 2.0.x"
            },
            "files": {
                "place": [
                    {
                        "src": "plug/*",
                        "dest": "plugins"
                    },
                    {
                        "src": "config.yml",
                        "dest": "config.yml"
                    }
                ],
                "preserve": [
                    "config.yml"
                ],
                "remove": [
                    "plugins/ExamplePlugin.dll"
                ]
            },
            "goos": "windows"
        },
        {
            "commands": {
                "pre-install": [
                    "echo Pre-install command for Linux AMD64"
                ]
            },
            "dependencies": {
                "github.com/tooth-hub/example-deps": ">=1.0.0 <=1.1.0 || 2.0.x"
            },
            "files": {
                "place": [
                    {
                        "src": "plug/*",
                        "dest": "plugins"
                    },
                    {
                        "src": "config.yml",
                        "dest": "config.yml"
                    }
                ],
                "preserve": [
                    "config.yml"
                ],
                "remove": [
                    "plugins/ExamplePlugin.dll"
                ]
            },
            "goos": "linux",
            "goarch": "amd64"
        }
    ]
}
```

### 注意事项

如果匹配了多个特定于平台的配置，最后一个将覆盖前面的配置。因此，您应该将最具体的配置放在数组的末尾。

如果设置了特定于平台的配置，则全局配置中的`commands`、`dependencies`和`files`将被忽略，无论它们在特定于平台的配置中是否设置。因此，如果您想设置特定于平台的配置，强烈建议不要在全局配置中设置它们。
