# lip cache

## Usage

```shell
lip cache add <package>
lip cache clean
lip cache list
```

## Description

Inspect and manage lip's cache.

## Sub-commands

### add

```shell
lip cache add <package>
```

Add a package to the cache.

`<package>` is a [package specifier](./lip-install.md#package-specifier).

If a Go module proxy is set in configuration, lip will download the package via Goproxy. Otherwise, lip will download the package directly from the Git repository.

### clean

```shell
lip cache clean
```

Remove all items from the cache.

### list

```shell
lip cache list
```

List items in the cache.
