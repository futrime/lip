# lip config

## Usage

```shell
lip config set <key>=<value> [<key>=<value> ...]
lip config get <key> [<key> ...]
lip config delete <key> [<key> ...]
lip config list
```

## Description

Manage the lip configuration files.

lip stores configuration files at `%APPDATA%\lip\liprc.json` for Windows and `~/.config/lip/liprc.json` for POSIX-like systems.

## Sub-commands

### set

```shell
lip config set <key>=<value> [<key>=<value> ...]
```

Set a configuration value. `<key>` is the configuration key, e.g. `cache.dir`. `<value>` is the configuration value, e.g. `~/.cache/lip`.

### get

```shell
lip config get [<key> [<key> ...]]
```

Get a configuration value. `<key>` is the configuration key, e.g. `cache.dir`.

### delete

```shell
lip config delete <key> [<key> ...]
```

Delete a configuration value. `<key>` is the configuration key, e.g. `cache.dir`.

### list

```shell
lip config list
```

List all configuration values.
