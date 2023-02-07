# 教程：将你的齿包提交给lip注册表

从v0.8.0开始，Lip支持从注册表中查找齿包并安装它们。你可以将你的齿包提交给注册表，使其他人更容易安装你的齿包。

在本教程中，我们将向官方Lip注册表（<https://registry.litebds.com>）提交一个齿包。

## 创造一个齿包

遵循 [创建一个齿包](tutorials/create_a_lip_tooth.md) 的指导创建一个齿包。请确保你在你的牙齿根目录下有一个`tooth.json`文件。同时确保你的齿包存储在其所声明的存储库中

## 将你的齿包提交给lip注册表

要向注册表提交你的牙齿，你需要向注册表存储库创建一个Pull Requests  (<https://github.com/LiteLDev/Registry>).

你应该在`teeth`目录下创建一个新的文件。文件名应该是你的牙齿的别名。文件内容应该是你的`tooth.json`的简化版本。文件内容应该是JSON格式，应该包含以下字段。在这个例子中，牙齿的别名是`lip`。因此，文件名是`lip.json`。

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
        "repository": "github.com/LiteLDev/Lip"
    }
}
```

与你的齿库下的`tooth.json`不同，注册表文件中的每个字段都是必须的。这些字段也应该遵循这些规则。

- `format_version`字段应该是`1`。
- `tooth`字段应该是不含协议前缀的牙齿资源库路径。目前，我们只接受托管在GitHub上的牙齿。
- `author`字段应该是牙齿作者的GitHub用户名。
- `description` 字段应该是对牙齿的单行描述。
- `homepage`字段应该是一个有效的URL，前缀为`http://`或`https://`。
- `license` 字段应该是有效的[SPDX许可证标识符](https://spdx.org/licenses/)（包括废弃的）。对于私有软件，请留空。
- `repository`文件应该是项目源代码库的路径，不含协议前缀。目前，我们只接受托管在GitHub上的仓库。

你可能想在注册表网站上显示一个README页面。你可以创建一个与注册表文件同名的Markdown文件。例如，你可以在`readmes`目录下创建一个`lip.md`文件。该文件的内容将显示在注册表网站上。

通过www.DeepL.com/Translator（免费版）翻译

在你创建文件之后，你可以向注册表仓库创建一个Pull Requests。注册表维护者将审查你的Pull Requests，如果它是有效的，则将其合并。

## 从Lip注册表安装你齿包

在你的齿包提交到注册表后，每个人都可以从注册表中安装它。你可以使用`lip install`命令，从注册表中安装你的齿包。

```bash
lip install exampletool
```

## 更新你的齿包在Lip注册表上的注册

如果你想在注册表中更新你的齿包，你需要更新注册文件。你可以以向注册表提交你更新注册文件。注册表维护者将审查你的Pull Requests，如果它是有效的，则将其合并。

## 从Lip注册表中删除你的齿包

如果你想从注册表中删除你的齿包，你需要删除注册表文件。你可以用提交你的齿包到注册表的同样方式来删除注册表文件。注册表维护者将审查你的Pull Requests，如果它是有效的，将其合并。