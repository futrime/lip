# lip view

## Usage

```shell
lip view <package> [path]
```

## Description

Show information about a package. If not cached, lip will download the package.

## Arguments

- `package`

  The package to view.

- `path`

  The path to the property to view. If not specified, the entire package information will be shown.

## Examples

```shell
lip view github.com/LiteLDev/LeviLamina@1.0.0 info.name
```
