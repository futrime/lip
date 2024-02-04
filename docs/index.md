# lip

[![Build](https://img.shields.io/github/actions/workflow/status/lippkg/lip/build.yml?style=for-the-badge)](https://github.com/lippkg/lip/actions)
[![Latest Tag](https://img.shields.io/github/v/tag/lippkg/lip?label=LATEST%20TAG&style=for-the-badge)](https://github.com/lippkg/lip/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/lippkg/lip/latest/total?style=for-the-badge)](https://github.com/lippkg/lip/releases/latest)

A general package installer

lip is a general package installer. You can use lip to install packages from any Git repository.

## Security

This software package manager (hereinafter referred to as "this software") is developed and provided by LiteLDev (hereinafter referred to as "the developer"). This software is designed to help users manage and install various software packages, but is not responsible for any content, quality, functionality, security or legality of any software package. Users should use this software at their own discretion and assume all related risks.

The developer does not guarantee the stability, reliability, accuracy or completeness of this software. The developer is not liable for any defects, errors, viruses or other harmful components that may exist in this software. The developer is not liable for any direct or indirect damages (including but not limited to data loss, device damage, profit loss etc.) caused by the use of this software.

The developer reserves the right to modify, update or terminate this software and its related services at any time without prior notice to users. Users should back up important data and check regularly for updates of this software.

Users should comply with relevant laws and regulations when using this software, respect the intellectual property rights and privacy rights of others, and not use this software for any illegal or infringing activities. If users violate the above provisions and cause any damage to any third party or are claimed by any third party, the developer does not bear any responsibility.

If you have any questions or comments about this disclaimer, please contact the developer.

## Install

First, download latest version of lip from <https://github.com/lippkg/lip/releases/latest>. You may select the version for your platform.

Then, unzip the content to somewhere you would like to install lip.

Finally, add the location to PATH environment variable.

To check if lip is installed successfully, run `lip --version` in your terminal. You should see the version of lip you just installed.

## Usage

To install a online tooth (a package in lip), run `lip install <tooth>`. Here is an example:

```bash
lip install github.com/tooth-hub/bdsdownloader
```

To install a local tooth (typically with `.tth` extension name), run `lip install <path>`. Here is an example:

```bash
lip install ./bdsdownloader.tth
```

To uninstall a tooth, run `lip uninstall <tooth>`. Here is an example:

```bash
lip uninstall github.com/tooth-hub/bdsdownloader
```

To list all installed teeth, run `lip list`. Here is an example:

To show information of a tooth, run `lip show <tooth>`. Here is an example:

```bash
lip show github.com/tooth-hub/bdsdownloader
```

## Contributing

Feel free to dive in! [Open an issue](https://github.com/lippkg/lip/issues/new/choose) or submit PRs.

lip follows the [Contributor Covenant](https://www.contributor-covenant.org/version/2/1/code_of_conduct/) Code of Conduct.

### Contributors

This project exists thanks to all the people who contribute.

![Contributors](https://contrib.rocks/image?repo=lippkg/lip)

## License

GPL-3.0-only © 2021-2024 lippkg
