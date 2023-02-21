# 教程：将你的tooth提交给lip注册表

从v0.8.0开始，Lip支持从注册表中查找tooth并安装它们。你可以将你的tooth提交给注册表，使其他人更容易安装你的tooth。

在本教程中，我们将向官方Lip注册表（<https://registry.litebds.com>）提交一个tooth。

## 创造一个tooth

遵循 [创建一个tooth](tutorials/create_a_lip_tooth.md) 的指导创建一个tooth。请确保你在你的tooth根目录下有一个`tooth.json`文件。同时确保你的tooth存储在其所声明的存储库中

## 将你的tooth提交给lip注册表

要向注册表提交你的tooth，你需要向注册表存储库创建一个Pull Requests  (<https://github.com/LiteLDev/Registry>).

你应该在`tooths`目录下创建一个新的文件。文件名应该是你的tooth的别名。文件内容应该是你的`tooth.json`的简化版本。文件内容应该是JSON格式，应该包含以下字段。在这个例子中，tooth的别名是`lip`。因此，文件名是`lip.json`。

```json
{
    "format_version": 1,
    "tooth": "github.com/Tooth-Hub/Lip",
    "information": {
        "author": "LiteLDev",
        "description": "A package installer not only for LiteLoaderBDS",
        "homepage": "https://www.example.com",
        "license": "MIT",
        "name": "Lip",
        "repository": "github.com/LiteLDev/Lip",
        "tags": ["utility", "package-manager"]
    }
}
```

The `format_version`, `tooth`, `author`, `description` and `name` fields are required. 这些字段也应该遵循这些规则。

- `format_version`字段应该是`1`。
- `tooth`字段应该是不含协议前缀的tooth资源库路径。目前，我们只接受托管在GitHub上的tooth。
- `author`字段应该是tooth作者的GitHub用户名。
- `description` 字段应该是对tooth的单行描述。
- `homepage`字段应该是一个有效的URL，前缀为`http://`或`https://`。
- `license` 字段应该是有效的[SPDX许可证标识符](https://spdx.org/licenses/)（包括废弃的）。对于私有软件，请留空。
- `repository`文件应该是项目源代码库的路径，不含协议前缀。目前，我们只接受托管在GitHub上的仓库。
- The `tags` field should be an array of strings. Each string should be a valid tag. The tag can only contain lowercase letters, numbers and hyphens [a-z0-9-]. The tag should not start or end with a hyphen. The tag should not contain consecutive hyphens.

你可能想在注册表网站上显示一个README页面。你可以创建一个与注册表文件同名的Markdown文件。例如，你可以在`readmes`目录下创建一个`lip.md`文件。该文件的内容将显示在注册表网站上。

通过www.DeepL.com/Translator（免费版）翻译

在你创建文件之后，你可以向注册表仓库创建一个Pull Requests。注册表维护者将审查你的Pull Requests，如果它是有效的，则将其合并。

### 如果设置标签

yphens [a-z0-9-]. The tag should not start or end with a hyphen. The tag should not contain consecutive hyphens.
你可以为你的tooth设置标签。这些标签将显示在Lip注册表网站上。你可以在注册表文件的`tags`字段中设置标签。`tags`字段应该是一个字符串数组。每个字符串都应该是一个有效的标签。标签只能包含小写字母、数字和连字符[a-z0-9-]。标签不应该以连字符开始或结束。标签不应包含连续的连字符。

有些标签可以被注册网站、Lip和LipUI识别。被识别的标签有：

保留的标签：

- `featured`: 这个tooth在注册表网站上被推荐。你不应该手动设置这个标签。注册表维护者将为你设置这个标签。

包种类标签：

- `utility`: 该tooth是一个实用工具。
- `plugin`: 该tooth是一个插件。
- `module`: 该tooth是一个模块
- `mod`: 该tooth是一个Mod
- `modpack`: 该tooth是一个MOD包。这个标签意味着该tooth是一个MOD的集合。
- `addon`: 牙齿是一个addon。
- `world`: 该tooth是一个游戏世界
- `integration`: 该tooth是一个整合包。这个标签意味着该tooth是一个MOD和插件以及服务器软件的集合。

生态系统标签：

- `ll`: 该tooth是用于LiteLoaderBDS的。
- `llse`: 该tooth是为LiteLoaderBDS设计的，并依赖于LiteLoaderBDS脚本引擎。
- `llnet`: 该tooth是为LiteLoaderBDS设计的，并依赖于LiteLoader.NET。
- `bdsx`: 这是一个bdsx的tooth包。
- `pnx`: 这是一个PowerNukkitX的tooth包
- `bds`: 这是一个原生BDS的tooth包。

## 从Lip注册表安装你tooth

在你的tooth提交到注册表后，每个人都可以从注册表中安装它。你可以使用`lip install`命令，从注册表中安装你的tooth。

```bash
lip install exampletool
```

## 更新你的tooth在Lip注册表上的注册

如果你想在注册表中更新你的tooth，你需要更新注册文件。你可以以向注册表提交你更新注册文件。注册表维护者将审查你的Pull Requests，如果它是有效的，则将其合并。

## 从Lip注册表中删除你的tooth

如果你想从注册表中删除你的tooth，你需要删除注册表文件。你可以用提交你的tooth到注册表的同样方式来删除注册表文件。注册表维护者将审查你的Pull Requests，如果它是有效的，将其合并。