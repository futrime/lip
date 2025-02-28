# lip update

## Usage

```shell
lip update <package>...
```

## Description

Update packages and their dependencies from various sources. Equivalent to `lip install --update <package...>`.

## Options

- `--dry-run`

  Do not actually update any packages. Be aware that files will still be downloaded and cached.

- `-f, --force`

  Force the installation of the package. When a package is already installed but its version is
  higher than the specified version, lip will still reinstall the package.

- `--ignore-scripts`

  Do not run any scripts during updating.

- `--no-dependencies`

  Bypass dependency resolution and only install the specified packages.
