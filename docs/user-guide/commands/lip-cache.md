# lip cache

## Usage

```shell
lip cache add <package-spec>
lip cache clean
lip cache list
```

## Description

Inspect and manage lip’s cache.

lip stores cache data in `%LocalAppData%\lip\cache` for Windows and `~/.local/share/lip/cache` for POSIX-like systems by default.

## Sub-commands

### add

```shell
lip cache add <package-spec>
```

Add a package to the cache. `<package-spec>` is a [package specifier](./lip-install.md#package-specifier).

If a Go module proxy is set in configuration, lip will download the package via Goproxy. Otherwise, lip will download the package directly from the Git repository.

### clean

```shell
lip cache clean
```

Remove all items from the cache, or if `<package-spec>` is specified, remove the specified item.

### list

```shell
lip cache list
```

List items in the cache.
