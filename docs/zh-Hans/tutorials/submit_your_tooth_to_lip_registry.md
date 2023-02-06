# 教程：将你的tooth包提交给lip注册表

从v0.8.0开始，Lip支持从注册表中查找tooth包并安装它们。你可以将你的tooth提交给注册表，使其他人更容易安装你的tooth包。

在本教程中，我们将向官方Lip注册表（<https://registry.litebds.com>）提交一个tooth包。

## 创造一个tooth包

遵循 [创建一个Tooth包](tutorials/create_a_lip_tooth.md) 的指导创建一个tooth包。请确保你在你的牙齿根目录下有一个`tooth.json`文件。同时确保你的tooth包存储在其所声明的存储库中

## 将你的tooth包提交给lip注册表

要向注册表提交你的牙齿，你需要向注册表存储库创建一个Pull Requests  (<https://github.com/LiteLDev/Registry>).

你应该在`teeth`目录下创建一个新的文件。文件名应该是你的牙齿的别名。文件内容应该是你的`tooth.json`的简化版本。文件内容应该是JSON格式，应该包含以下字段。在这个例子中，牙齿的别名是`exampletool`。因此，文件名是`exampletool.json`。

```json
{
    "format_version": 1,
    "tooth": "example.com/exampleuser/exampletool",
    "information": {
        "name": "Example Tool",
        "description": "An example tool",
        "author": "Example User",
        "license": "MIT",
        "homepage": "example.com"
    }
}
```

与你的tooth存储库下的`tooth.json`不同，注册表文件中的每个字段都是必须的。`format_version`字段应该是`1'。

在你创建文件之后，你可以向注册表仓库创建一个Pull Requests。注册表维护者将审查你的Pull Requests，如果它是有效的，则将其合并。

## 从Lip注册表安装你tooth包

在你的tooth包提交到注册表后，每个人都可以从注册表中安装它。你可以使用`lip install`命令，从注册表中安装你的tooth包。

```bash
lip install exampletool
```

## 更新你的tooth包在Lip注册表上的注册

如果你想在注册表中更新你的tooth包，你需要更新注册文件。你可以以向注册表提交你更新注册文件。注册表维护者将审查你的Pull Requests，如果它是有效的，则将其合并。

## 从Lip注册表中删除你的tooth包

如果你想从注册表中删除你的tooth包，你需要删除注册表文件。你可以用提交你的tooth包到注册表的同样方式来删除注册表文件。注册表维护者将审查你的Pull Requests，如果它是有效的，将其合并。