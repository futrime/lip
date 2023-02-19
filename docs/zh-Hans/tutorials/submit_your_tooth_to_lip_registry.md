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

### How to Set Tags

You can set tags for your tooth. The tags will be displayed on the registry website. You can set tags in the `tags` field of the registry file. The `tags` field should be an array of strings. Each string should be a valid tag. The tag can only contain lowercase letters, numbers and hyphens [a-z0-9-]. The tag should not start or end with a hyphen. The tag should not contain consecutive hyphens.

Some tags can be recognized by the registry website, Lip and LipUI. The recognized tags are:

Reserved tags:

- `featured`: 这个tooth在注册表网站上被推荐。你不应该手动设置这个标签。注册表维护者将为你设置这个标签。

Type tags:

- `utility`: The tooth is a utility tool.
- `plugin`: The tooth is a plugin.
- `module`: The tooth is a module. This 
- `mod`: The tooth is a mod.
- `modpack`: The tooth is a modpack. This tag means that the tooth is a collection of mods.
- `addon`: The tooth is an addon.
- `world`: The tooth is a world.
- `integration`: The tooth is an integration pack. This tag means that the tooth is a collection of mods and plugins as well as the server software.

Ecosystem tags:

- `ll`: The tooth is for LiteLoaderBDS.
- `llse`: The tooth is for LiteLoaderBDS and depends on LiteLoaderBDS Script Engine.
- `llnet`: The tooth is for LiteLoaderBDS and depends on LiteLoader.NET.
- `bdsx`: The tooth is for BDSX.
- `pnx`: The tooth is for PowerNukkitX.
- `bds`: The tooth is for pure BDS.

## 从Lip注册表安装你tooth

在你的tooth提交到注册表后，每个人都可以从注册表中安装它。你可以使用`lip install`命令，从注册表中安装你的tooth。

```bash
lip install exampletool
```

## 更新你的tooth在Lip注册表上的注册

如果你想在注册表中更新你的tooth，你需要更新注册文件。你可以以向注册表提交你更新注册文件。注册表维护者将审查你的Pull Requests，如果它是有效的，则将其合并。

## 从Lip注册表中删除你的tooth

如果你想从注册表中删除你的tooth，你需要删除注册表文件。你可以用提交你的tooth到注册表的同样方式来删除注册表文件。注册表维护者将审查你的Pull Requests，如果它是有效的，将其合并。