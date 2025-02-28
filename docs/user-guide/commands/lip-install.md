# lip install

## Usage

```shell
lip install [<package> ...]
```

## Description

Install packages and their dependencies from various sources.

A `<package>` can be any of the following (in order of priority):

- A directory containing a `tooth.json` file
- An archive (`.zip`, `.tar`, `.tgz` or `.tar.gz`) containing a directory with a `tooth.json` file
- A [package specifier](#package-specifier) referencing a Git repository

If no `<package>` is specified and the current directory contains a `tooth.json` file, lip will install the package in the current directory. Inplace file placement will not be performed.

When using a package specifier, lip will use Goproxy to download the package if a Go module proxy is set in configuration. Otherwise, packages are downloaded directly from their Git repositories.

When resolving dependencies, lip follows these rules:

- Selects the latest stable version that meets the specified constraints
- Falls back to the latest pre-release version if no stable version is available
- Installs dependencies in topological order (dependencies before dependents)
- Rejects installation if it detects circular dependencies

lip maintains a dependency graph to track relationships between packages. When uninstalling packages, lip checks this graph to ensure all dependent packages are handled appropriately. If dependents are found, you'll be prompted to either uninstall them or cancel the operation.

Pre-release versions can be installed by explicitly specifying the version number. While packages can declare pre-release versions as dependencies, lip ignores pre-release versions when evaluating version ranges or wildcards.

## Package Specifier

A package specifier is a string that identifies a package's [tooth path](../files/tooth-json.md#tooth-required), an optional variant, and a version.

The format is `<tooth-path>[#<variant>]@<version>`.

- `<tooth-path>` is the tooth path of the package.
- `<variant>` is the label of the package variant to use. If omitted, lip will use the default variant.
- `<version>` is the version of the package to use.

Examples:

- `github.com/futrime/example-package@1.0.0`
- `github.com/futrime/example-package#variant_1@1.0.0`

## Options

- `--dry-run`

  Do not actually install any packages. Be aware that files will still be downloaded and cached.

- `-f, --force`

  Force the installation of the package. When a package is already installed but its version is different from the specified version, lip will reinstall the package.

- `--ignore-scripts`

  Do not run any scripts during installation.

- `--no-dependencies`

  Bypass dependency resolution and only install the specified packages.
