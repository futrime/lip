# lip

[![构建](https://img.shields.io/github/actions/workflow/status/lippkg/lip/build.yml?style=for-the-badge)](https://github.com/lippkg/lip/actions)
[![最新标签](https://img.shields.io/github/v/tag/lippkg/lip?label=LATEST%20TAG&style=for-the-badge)](https://github.com/lippkg/lip/releases/latest)
[![下载次数](https://img.shields.io/github/downloads/lippkg/lip/latest/total?style=for-the-badge)](https://github.com/lippkg/lip/releases/latest)

一个通用的软件包安装程序

lip是一个通用的软件包安装程序。您可以使用lip从任何Git存储库安装软件包。

## 安全性

此软件包管理器（以下简称“本软件”）由lippkg（以下简称“开发者”）开发和提供。本软件旨在帮助用户管理和安装各种软件包，但不对任何软件包的内容、质量、功能、安全性或合法性负责。用户应自行谨慎使用本软件并承担所有相关风险。

开发者不保证本软件的稳定性、可靠性、准确性或完整性。开发者对本软件中可能存在的任何缺陷、错误、病毒或其他有害组件不负责任。开发者不对因使用本软件而导致的任何直接或间接损害（包括但不限于数据丢失、设备损坏、利润损失等）负责。

开发者保留随时修改、更新或终止本软件及其相关服务的权利，而无需事先通知用户。用户应定期备份重要数据并检查本软件的更新。

用户在使用本软件时应遵守相关法律法规，尊重他人的知识产权和隐私权，不得将本软件用于任何非法或侵权活动。如果用户违反上述规定并对任何第三方造成损害或被任何第三方索赔，开发者不承担任何责任。

如果您对本免责声明有任何疑问或意见，请联系开发者。

## 安装

首先，从 <https://github.com/lippkg/lip/releases/latest> 下载lip的最新版本。您可以选择适合您平台的版本。

然后，解压内容到您想要安装lip的位置。

最后，将该位置添加到PATH环境变量中。

要检查是否成功安装lip，请在终端中运行 `lip --version`。您应该看到刚刚安装的lip的版本。

如果你使用的是Windows系统，你也可以在Assets中下载 `.exe` 后缀的安装程序来安装。

## 使用

要安装在线tooth（lip中的软件包），运行 `lip install <tooth>`。以下是一个示例：

```bash
lip install github.com/LiteLDev/LeviLamina
```

要安装本地tooth（通常使用`.tth`扩展名），运行 `lip install <path>`。以下是一个示例：

```bash
lip install ./example.tth
```

要卸载tooth，运行 `lip uninstall <tooth>`。以下是一个示例：

```bash
lip uninstall github.com/LiteLDev/LeviLamina
```

要列出所有安装的tooth，运行 `lip list`。以下是一个示例：

要显示tooth的信息，运行 `lip show <tooth>`。以下是一个示例：

```bash
lip show github.com/LiteLDev/LeviLamina
```

## 贡献

随时加入！[提出问题](https://github.com/lippkg/lip/issues/new/choose) 或提交PR。

lip遵循[贡献者公约](https://www.contributor-covenant.org/version/2/1/code_of_conduct/)行为准则。

### 贡献者

感谢所有为此项目做出贡献的人。

![贡献者](https://contrib.rocks/image?repo=lippkg/lip)

## 许可证

GPL-3.0-only © 2021-2024 lippkg
