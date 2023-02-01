# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.7.0] - 2023-02-01

### Added

- Support for installing anything to any path.
- Prompt for confirmation when installing to a path that is not in working directory.

## [0.6.0] - 2023-01-31

### Added

- Support for on-demand installation depending on OS and platform.
- Removal for downloaded tooth files that do not pass the validation.

## [0.5.1] - 2023-01-30

### Fixed

- Failing to install any tool.

## [0.5.0] - 2023-01-30

### Added

- Available version list in `lip show` command.
- Redirection to local lip executable when running `lip`.
- Support for pre-uninstall scripts.
- Support for hot update of lip.
- Support for executing tools in `.lip/tools` directory.

## [0.4.0] - 2023-01-26

### Added

- Post-install script support.
- Tooth path validation.
- Flexible tooth.json parsing.

## [0.3.4] - 2023-01-25

### Changed

- Bumped github.com/fatih/color from 1.14.0 to 1.14.1.

### Fixed

- Misleading error hints.
- Failing to fetch tooth with major version v0 or v1.
- Failing to match dependencies.
- Failing to fetch tooth when uppercase letters exist in tooth path.

## [0.3.3] - 2023-01-24

### Fixed

- Default to earliest version when no version is specified in tooth.json.
- Panic when tooth.json is invalid.

## [0.3.2] - 2023-01-23

### Added

- "Add to PATH" option in setup utility.
- Mac OS, Linux and OpenBSD support.
- Arm64 support.

## [0.3.1] - 2023-01-21

### Added

- Setup utility to install Lip.

## [0.3.0] - 2023-01-20

### Added

- Possession keeping support when force-reinstalling or upgrading.
- `--force-reinstall` flag and `--upgrade` flag support.

## [0.2.1] - 2023-01-18

### Fixed

- Failing to fetch tooth whose version has suffix `+incompatible`.
- Failing to parse wildcards.

## [0.2.0] - 2023-01-18

### Added

- Possession field in tooth.json to specify directory to remove when uninstalling a tooth.

### Fixed

- Fix failing to fetch tooth when the repository does not contain go.mod file.
- Fix failing to parse tooth file when the tooth is downloaded via GOPROXY.
- Fix failing to parse tooth when tooth.json is the only file in the tooth.

### Changed

- Change extension name of tooth files to .tth

## [0.1.0] - 2023-01-17

### Added

- Basic functions: cache, install, list, show, tooth init, and uninstall.

[unreleased]: https://github.com/LiteLDev/Lip/compare/v0.7.0...HEAD
[0.7.0]: https://github.com/LiteLDev/Lip/releases/tag/v0.6.0...v0.7.0
[0.6.0]: https://github.com/LiteLDev/Lip/releases/tag/v0.5.1...v0.6.0
[0.5.1]: https://github.com/LiteLDev/Lip/releases/tag/v0.4.0...v0.5.1
[0.5.0]: https://github.com/LiteLDev/Lip/releases/tag/v0.4.0...v0.5.0
[0.4.0]: https://github.com/LiteLDev/Lip/releases/tag/v0.3.4...v0.4.0
[0.3.4]: https://github.com/LiteLDev/Lip/releases/tag/v0.3.3...v0.3.4
[0.3.3]: https://github.com/LiteLDev/Lip/releases/tag/v0.3.2...v0.3.3
[0.3.2]: https://github.com/LiteLDev/Lip/releases/tag/v0.3.1...v0.3.2
[0.3.1]: https://github.com/LiteLDev/Lip/releases/tag/v0.3.0...v0.3.1
[0.3.0]: https://github.com/LiteLDev/Lip/releases/tag/v0.2.1...v0.3.0
[0.2.1]: https://github.com/LiteLDev/Lip/releases/tag/v0.2.0...v0.2.1
[0.2.0]: https://github.com/LiteLDev/Lip/releases/tag/v0.1.0...v0.2.0
[0.1.0]: https://github.com/LiteLDev/Lip/releases/tag/v0.1.0
