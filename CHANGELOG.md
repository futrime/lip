# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.22.1] - 2024-07-12

### Fixed

- Fix `.lip` path in linux [#129]
- Fix unable to decompress .tar.gz [#140]
- Delete surplus tabs (#139)

## [0.22.0] - 2024-03-23

### Added

- `--no-color` flag to disable color output.
- NSIS installer.

## [0.21.2] - 2024-02-21

### Fixed

- Some ambiguous error messages.
- Not prompting for confirmation to overwrite existing files when installing a tooth.

## [0.21.1] - 2024-02-17

### Fixed

- Untidy error messages.
- Failed to install any tooth without setting a proxy.

## [0.21.0] - 2024-02-17

### Added

- Proxy configuration support.
- Support more kinds of GitHub mirrors
- Support `NO_COLOR` environment variable to disable color output.

### Changed

- Set `https://github.com` as the default GitHub mirror.

### Fixed

- Panic when dependencies and prerequisites have malformed version strings.

## [0.20.1] - 2024-02-04

### Fixed

- Version string.

## [0.20.0] - 2024-02-03

### Added

- Support Go module URL as asset URL.

### Changed

- Files downloaded from GitHub mirror are now cached in different paths.

### Removed

- `source` field in `tooth.json`.

## [0.19.0] - 2024-01-20

### Added

- Tons of debug logging.
- Show available versions without installing.

### Changed

- Optimize error logging.

### Fixed

- Wildcard not parsing correctly.
- Wrong error output when "%" is used in messages.
- Wrong version `@0.0.0` when stringify specifiers without version.

## [0.18.1] - 2024-01-12

### Fixed

- Platform-specific items in tooth.json.
- Go module path escaping.

## [0.18.0] - 2024-01-12

### Added

- Config support.
- GitHub mirror support.

### Changed

- Use `github.com/sirupsen/logrus` for logging.
- Use `github.com/blang/semver/v4` for versioning.
- Refactor most of the code.

### Removed

- Remove `lip autoremove` command.
- Percentage progress bar.

### Fixed

- Some bugs.

## [0.17.0] - 2023-12-30

### Added

- Warning for deprecated tooth.json format versions.
- `info.source` field for indicating the source repo of a tooth.

### Changed

- Check tooth repo validity with Go official module library.

### Fixed

- Failed to uninstall a tooth when some of its files are already removed.

## [0.16.1] - 2023-10-11

### Fixed

- Unable to specify which version to install.

## [0.16.0] - 2023-10-09

### Added

- Prerequisite support.

### Fixed

- Failing to parse files under root directory of a tooth.

## [0.15.2] - 2023-09-05

### Fixed

- Inconsistent tooth.json schema with lipIndex.
- Wrong help messages.

## [0.15.1] - 2023-09-01

### Fixed

- Wrong help message for `--json` flag of `lip show` command.
- Wrong regex for validating versions.

## [0.15.0] - 2023-06-23

### Added

- Tooth file preservation and removal.

### Changed

- Optimize output information format.
- Change tooth.json format to version 2.
- Force to specify output path in `lip tooth pack` command.
- Refactor all code and optimize performance.

### Removed

- `lip exec` command.
- Tooth file possession (use "preserve" and "remove" instead).
- Number-only progress bar.
- Registry support.
- Placed files display in `lip show` command.

## [0.14.2] - 2023-04-24

### Fixed

- Remove misleading message when dependencies are satisfied.
- Packed tooth file not able to be installed.

## [0.14.1] - 2023-04-20

### Fixed

- Broken GOOS and GOARCH specifiers of placement.

## [0.14.0] - 2023-03-24

### Added

- `lip tooth pack` command to pack a tooth into a tooth file.
- Questions when initializing a tooth.

### Fixed

- Not properly parsing placement sometimes.
- Not aborting when a tooth fails to install.

## [0.13.0] - 2023-03-05

### Added

- Aliases for subcommands.

### Changed

- Tool should be registered in `tooth.json` manually now.

## [0.12.0] - 2023-02-27

### Added

- Dependency version validation when installing.
- `lip autoremove` command to remove teeth not required by other teeth.

### Fixed

- Failing to run tools with arguments.
- Dependencies still being installed when the dependent is not going to be installed.

## [0.11.45141] - 2023-02-19

### Fixed

- Wrongly showing debug information when redirecting to local lip.

## [0.11.4514] - 2023-02-17

### Added

- Showing information about installed teeth in `lip list` command.

## [0.11.0] - 2023-02-17

### Added

- `--available` flag for `lip show` command.
- `--numeric-progress` flag for `lip install` command.
- `--no-dependencies` flag for `lip install` command.
- `confirmation` field in `tooth.json` to show messages and ask for confirmation before installing.
- Check for invalid additional arguments.
- Structured information output.
- Support for multiple GOPROXYs.
- `--keep-possession` flag for `lip uninstall` command.
- Automatic deletion of empty directories when uninstalling a tooth.
- Support for file possession.

### Fixed

- Remove wrongly displayed debug information.
- Failing to re-download when a broken tooth file exists in cache.

## [0.10.0] - 2023-02-12

### Added

- Verbose and quiet mode.
- JSON output support for `lip list` and `lip show`.

## [0.9.0] - 2023-02-11

### Added

- HTTP Code reporting when failing to make a request.
- `lip list --upgradable` command.
- Topological sorting for dependencies.
- Progress bar for downloading tooth files.

### Fixed

- No notice when a tooth file is cached.
- Tooth paths in `dependencies` field of `tooth.json` not converting to lowercase.
- Mistakes in help message of `lip cache purge`.

## [0.8.3] - 2023-02-09

### Fixed

- Mistakes in path prefix extraction when there is only one file in the tooth.

## [0.8.2] - 2023-02-09

### Fixed

- Failing to input anything to post-install and pre-uninstall commands.
- Wrong installation order of dependencies.
- Registry not working in `lip show`.
- Unstable versions can be wrongly installed when no specific version is specified.

## [0.8.1] - 2023-02-07

### Fixed

- Failing to get information from registry with other index than index.json.

## [0.8.0] - 2023-02-06

### Added

- Registry support. The default registry is <https://registry.litebds.com>.

### Fixed

- Failing to uninstall teeth with uppercase letters in provided tooth path.

## [0.7.1] - 2023-02-05

### Fixed

- Failing to hot update or remove lip in local directory.

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

- Setup utility to install lip.

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

### Changed

- Change extension name of tooth files to .tth

### Fixed

- Fix failing to fetch tooth when the repository does not contain go.mod file.
- Fix failing to parse tooth file when the tooth is downloaded via GOPROXY.
- Fix failing to parse tooth when tooth.json is the only file in the tooth.

## [0.1.0] - 2023-01-17

### Added

- Basic functions: cache, install, list, show, tooth init, and uninstall.

[#129]: https://github.com/lippkg/lip/issues/129
[#140]: https://github.com/lippkg/lip/issues/140

[Unreleased]: https://github.com/lippkg/lip/compare/v0.22.1...HEAD
[0.22.1]: https://github.com/lippkg/lip/compare/v0.22.0...v0.22.1
[0.22.0]: https://github.com/lippkg/lip/compare/v0.21.2...v0.22.0
[0.21.2]: https://github.com/lippkg/lip/compare/v0.21.1...v0.21.2
[0.21.1]: https://github.com/lippkg/lip/compare/v0.21.0...v0.21.1
[0.21.0]: https://github.com/lippkg/lip/compare/v0.20.1...v0.21.0
[0.20.1]: https://github.com/lippkg/lip/compare/v0.20.0...v0.20.1
[0.20.0]: https://github.com/lippkg/lip/compare/v0.19.0...v0.20.0
[0.19.0]: https://github.com/lippkg/lip/compare/v0.18.1...v0.19.0
[0.18.1]: https://github.com/lippkg/lip/compare/v0.18.0...v0.18.1
[0.18.0]: https://github.com/lippkg/lip/compare/v0.17.0...v0.18.0
[0.17.0]: https://github.com/lippkg/lip/compare/v0.16.1...v0.17.0
[0.16.1]: https://github.com/lippkg/lip/compare/v0.16.0...v0.16.1
[0.16.0]: https://github.com/lippkg/lip/compare/v0.15.2...v0.16.0
[0.15.2]: https://github.com/lippkg/lip/compare/v0.15.1...v0.15.2
[0.15.1]: https://github.com/lippkg/lip/compare/v0.15.0...v0.15.1
[0.15.0]: https://github.com/lippkg/lip/compare/v0.14.2...v0.15.0
[0.14.2]: https://github.com/lippkg/lip/compare/v0.14.1...v0.14.2
[0.14.1]: https://github.com/lippkg/lip/compare/v0.14.0...v0.14.1
[0.14.0]: https://github.com/lippkg/lip/compare/v0.13.0...v0.14.0
[0.13.0]: https://github.com/lippkg/lip/compare/v0.12.0...v0.13.0
[0.12.0]: https://github.com/lippkg/lip/compare/v0.11.45141...v0.12.0
[0.11.45141]: https://github.com/lippkg/lip/compare/v0.11.4514...v0.11.45141
[0.11.4514]: https://github.com/lippkg/lip/compare/v0.11.0...v0.11.4514
[0.11.0]: https://github.com/lippkg/lip/compare/v0.10.0...v0.11.0
[0.10.0]: https://github.com/lippkg/lip/compare/v0.9.0...v0.10.0
[0.9.0]: https://github.com/lippkg/lip/compare/v0.8.3...v0.9.0
[0.8.3]: https://github.com/lippkg/lip/compare/v0.8.2...v0.8.3
[0.8.2]: https://github.com/lippkg/lip/compare/v0.8.1...v0.8.2
[0.8.1]: https://github.com/lippkg/lip/compare/v0.8.0...v0.8.1
[0.8.0]: https://github.com/lippkg/lip/compare/v0.7.1...v0.8.0
[0.7.1]: https://github.com/lippkg/lip/compare/v0.7.0...v0.7.1
[0.7.0]: https://github.com/lippkg/lip/compare/v0.6.0...v0.7.0
[0.6.0]: https://github.com/lippkg/lip/compare/v0.5.1...v0.6.0
[0.5.1]: https://github.com/lippkg/lip/compare/v0.5.0...v0.5.1
[0.5.0]: https://github.com/lippkg/lip/compare/v0.4.0...v0.5.0
[0.4.0]: https://github.com/lippkg/lip/compare/v0.3.4...v0.4.0
[0.3.4]: https://github.com/lippkg/lip/compare/v0.3.3...v0.3.4
[0.3.3]: https://github.com/lippkg/lip/compare/v0.3.2...v0.3.3
[0.3.2]: https://github.com/lippkg/lip/compare/v0.3.1...v0.3.2
[0.3.1]: https://github.com/lippkg/lip/compare/v0.3.0...v0.3.1
[0.3.0]: https://github.com/lippkg/lip/compare/v0.2.1...v0.3.0
[0.2.1]: https://github.com/lippkg/lip/compare/v0.2.0...v0.2.1
[0.2.0]: https://github.com/lippkg/lip/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/lippkg/lip/releases/tag/v0.1.0
