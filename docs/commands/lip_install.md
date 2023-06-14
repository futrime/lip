# lip install

## Usage

```shell
lip install [options] <requirement specifiers>
lip install [options] <tooth url/files>

aliases: add, i
```

## Description

Install a tooth from:

- tooth repositories via Goproxy.
- local or remote standalone tooth files (with suffix `.tth`).

For the tooth repository, you can specific the version by add suffix like `@1.2.3` or `@1.2.0-beta.3`. However, when another version is installed and you run Lip without `--upgrade` or `--force-reinstall` flag, Lip will not install the specific version.

Only lowercase letters, numbers, dashes, underlines, dots, slashes [a-z0-9-_./] and one @ are allowed in requirement specifiers.

If you have set environment variable GOPROXY, Lip will access tooth repositories via it. Otherwise, Lip will choose the default Goproxy <https://goproxy.io>.

### Overview

`lip install` has several stages:

1. Identify the base requirements. The user supplied arguments are processed here.
2. Fetch tooths and resolve dependencies. Dependencies will be resolved as soon as tooths are fetched.
3. Install the tooths (and uninstall anything being upgraded)

Note that `lip install` prefers to leave the installed version as-is unless `--upgrade` is specified.

### Argument Handling

When looking at the items to be installed, Lip checks what type of item each is, in the following order:

1. Remote tooth file with suffix `.tth` and prefix `http://` or `https://`.
2. Local tooth file with suffix `.tth`.
3. Tooth repository, which can be accessed via Goproxy.
4. Tooth alias, which can be looked up in Lip registry.

In 3 and 4, all letters will be converted to lowercase before processing.

### Lip Registry

Since v0.8.0, Lip supports Lip registry, which enables you to use aliases to install tooths. By default, Lip will use the registry at <https://registry.litebds.com>. You can also use your own registry by setting environment variable `LIP_REGISTRY` to the URL of your registry.

### Satisfying Requirements

Once Lip has the set of requirements to satisfy, it chooses which version of each requirement to install using the simple rule that the latest stable version that satisfies the given constraints will be installed.

### Installation Order

Lip installs dependencies before their dependents, i.e. in “topological order”. When encountering a cycle in the dependency graph, Lip will refuse to install tooths. All developers should avoid any cycle in the dependency graph.

This dependency graph will be maintained by Lip. When uninstalling some packages, Lip will check the graph to ensure that all dependents uninstalled. If not, Lip will ask you whether to uninstall them or cancel the procedure.

### Pre-release Versions

You can install any pre-release versions by specifying the version. And tooths can declare pre-release versions as their dependencies. However, when tooths use any type of range version match or wildcard, Lip will ignore pre-release versions.

## Options

- `-h, --help`

  Show help.

- `--upgrade`

  Upgrade the specified tooth to the newest available version. If a version is specified and it is newer, upgrade to that version. The handling of dependencies depends on the upgrade-strategy used. When upgrading, Lip will first uninstall the old version and then install the new version.

- `--force-reinstall`

  Reinstall the tooth even if they are already up-to-date. When reinstalling, Lip will first uninstall the tooth and then install it. If version specified, Lip will install the version, otherwise the newest version.

- `-y, --yes`

  Assume yes to all prompts and run non-interactively.

- `--numeric-progress`

  Show numeric progress instead of progress bar.

- `--no-dependencies`

  Do not install dependencies.

## Examples

Install from tooth repositories:

```shell
lip install example.com/some_user/some_tooth         # Latest version
lip install example.com/some_user/some_tooth@1.0.0   # Specific version
lip install github.com/LiteLDev/LiteLoaderBDS@2.11.0 # LiteLoderBDS 2.11.0
```

Upgrade an already installed tooth:

```shell
lip install --upgrade example.com/some_user/some_tooth
```

Force reinstall a tooth:

```shell
lip install --force-reinstall example.com/some_user/some_tooth
```

Install from URL of a tooth:

```shell
lip install https://example.com/example.tth
```

Install from a local tooth:

```shell
lip install example.tth
lip install ./example/example.tth
```

Install with an alias:

```shell
lip install liteloaderbds
```