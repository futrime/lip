# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unrelease]

## [0.2.1] - 2023-01-11

## Added

- go.mod creation when running `lip tooth init`.

## [0.2.0] - 2023-01-11

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

[unreleased]: https://github.com/LiteLDev/Lip/compare/v0.1.0...HEAD
[0.2.0]: https://github.com/LiteLDev/Lip/releases/tag/v0.1.0...v0.2.0
[0.1.0]: https://github.com/LiteLDev/Lip/releases/tag/v0.1.0