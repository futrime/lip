# lip uninstall

## Usage

```shell
lip uninstall <package-spec-without-version> ...
```

## Description

Uninstall packages.

## Options

- `--dry-run`

  Do not actually uninstall any packages.

- `--ignore-scripts`

  Do not run any scripts during installation.

- `--save`

  Remove the dependency item of the package in `tooth.json`. Only apply to default variant.
