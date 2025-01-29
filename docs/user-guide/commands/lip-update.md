# lip update

## Usage

```shell
lip update [<package-spec>...]
```

## Description

Attempt to update the specified packages to the specified versions. If no package specs are provided, lip will update all packages to the latest versions.

## Options

- `--dry-run`

  Do not actually update any packages. Be aware that files will still be downloaded and cached.

- `-f, --force`

  Force the update of the specified packages. This may break the dependency graph and cause all future installations and updates without `--force` to fail.

- `--ignore-scripts`

  Do not run any scripts during updating.

- `--no-dependencies`

  Do not update dependencies.

- `--save`

  Save the updated packages to `tooth.json`.
