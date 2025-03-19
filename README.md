# lip

[![Latest Tag](https://img.shields.io/github/v/tag/futrime/lip?style=for-the-badge)](https://github.com/futrime/lip/releases/latest)
[![GitHub License](https://img.shields.io/github/license/futrime/lip?style=for-the-badge)](https://github.com/futrime/lip/blob/main/COPYING)
[![Downloads](https://img.shields.io/github/downloads/futrime/lip/latest/total?style=for-the-badge)](https://github.com/futrime/lip/releases/latest)
[![Build](https://img.shields.io/github/actions/workflow/status/futrime/lip/build.yml?style=for-the-badge)](https://github.com/futrime/lip/actions/workflows/build.yml)

A general package installer

**lip** is a general package installer. You can use **lip** to install packages from any Git repository.

## Security

This software package manager (hereinafter referred to as "this software") is developed and provided by Zijian Zhang (hereinafter referred to as "the developer"). This software is designed to help users manage and install various software packages, but is not responsible for any content, quality, functionality, security or legality of any software package. Users should use this software at their own discretion and assume all related risks.

The developer does not guarantee the stability, reliability, accuracy or completeness of this software. The developer is not liable for any defects, errors, viruses or other harmful components that may exist in this software. The developer is not liable for any direct or indirect damages (including but not limited to data loss, device damage, profit loss etc.) caused by the use of this software.

The developer reserves the right to modify, update or terminate this software and its related services at any time without prior notice to users. Users should back up important data and check regularly for updates of this software.

Users should comply with relevant laws and regulations when using this software, respect the intellectual property rights and privacy rights of others, and not use this software for any illegal or infringing activities. If users violate the above provisions and cause any damage to any third party or are claimed by any third party, the developer does not bear any responsibility.

If you have any questions or comments about this disclaimer, please contact the developer.

## Install

**lip** is a standalone executable, so installation isn’t always necessary. Simply download the latest version from [here](https://github.com/futrime/lip/releases/latest). However, we do offer scripts and installers for those who prefer a more conventional setup.

For Linux and macOS, we provide installation scripts. Just run the appropriate command below and follow the on-screen instructions:

```shell
# For Linux
curl -fsSL https://raw.githubusercontent.com/futrime/lip/HEAD/scripts/install_linux.sh | sh
```

```shell
# For macOS
curl -fsSL https://raw.githubusercontent.com/futrime/lip/HEAD/scripts/install_macos.sh | sh
```

For Windows, you can install lip by winget:

```shell
winget install futrime.lip
```

or download either `lip-cli-win-x64-en-US.msi` (for English) or `lip-cli-win-x64-zh-CN.msi` (for Chinese) from [this page](https://github.com/futrime/lip/releases/latest). Run the installer and follow the prompts to set up **lip**.

## Usage

```shell
lip <command>
```

Check out [the documentation](https://futrime.github.io/lip/) for more information.

> [!NOTE]
> Looking for **lip** version 0.24.x or earlier? Check out [this link](https://github.com/futrime/lip/tree/v0.24.0).

## Contributing

Feel free to dive in! [Open an issue](https://github.com/futrime/lip/issues/new/choose) or submit PRs.

**lip** follows the [Contributor Covenant](https://www.contributor-covenant.org/version/2/1/code_of_conduct/) Code of Conduct.

### Contributors

This project exists thanks to all the people who contribute.

![Contributors](https://contrib.rocks/image?repo=futrime/lip)

## License

GPL-3.0-only © 2023-2025 Zijian Zhang
