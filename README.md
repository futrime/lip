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

**lip** is a self-contained executable file, so you may not want to install it. Just download the latest version from <https://github.com/futrime/lip/releases/latest>.

We also provide some scripts to help you install **lip**. You can run the following command to install **lip**:

For Windows (x64), run the following commands:
  
```shell
mkdir -p %LocalAppData%\lip
curl -L https://github.com/futrime/lip/releases/latest/download/lip-win-x64.zip -o %LocalAppData%\lip\lip.zip
tar -xf %LocalAppData%\lip\lip.zip -C %LocalAppData%\lip
del %LocalAppData%\lip\lip.zip
setx PATH "%PATH%;%LocalAppData%\lip"
```

For Windows (arm64), simply replace `lip-win-x64.zip` with `lip-win-arm64.zip` in the above commands.

For Linux, run the following command and follow the instructions in the script to complete the installation:
  
```shell
curl -fsSL https://raw.githubusercontent.com/futrime/lip/HEAD/scripts/install_linux.sh | sh
```

For macOS, run the following command and follow the instructions in the script to complete the installation:
  
```shell
curl -fsSL https://raw.githubusercontent.com/futrime/lip/HEAD/scripts/install_macos.sh | sh
```

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
