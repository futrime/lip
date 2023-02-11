# Getting Started

To get started with using Lip, you should [have Lip installed](installation.md) first.

## Ensure you have a working Lip

As a first step, you should check that you have a working Lip installed. This can be done by running the following commands and making sure that the output looks similar.

```shell
> lip --version
Lip 0.1.0 from C:\Users\ExampleUser\AppData\Local\Lip\lip.exe
```

## Common Tasks

### Install a tooth

```shell
> lip install github.com/liteldev/exampletooth@1.0.0
[...]
Successfully installed all tooth files.
```

By default, Lip will fetch tooths via GOPROXY, a proxy of Git repos.

### Install a tooth from URL

```shell
> lip install https://example.com/exampletooth.tth
[...]
Successfully installed all tooth files.
```

Lip only supports URLs started with "http://" or "https://". All URLs should ends with ".tth".

### Install a tooth from a tooth file

```shell
> lip install exampletooth.tth
[...]
Successfully installed all tooth files.
```

The tooth file should have ".tth" extension name.

### Install multiple tooths

Lip suppports installing multiple files at a time.

```shell
> lip install github.com/liteldev/exampletooth@1.0.0 github.com/liteldev/anotherexampletooth@1.0.0
[...]
Successfully installed all tooth files.
```

### Upgrade a tooth

```shell
> lip install --upgrade github.com/liteldev/exampletooth
[...]
Successfully installed all tooth files.
```

### Uninstall a tooth

To uninstall a tooth, you must provide the tooth path of the tooth.

```shell
> lip uninstall github.com/liteldev/exampletooth
[...]
Successfully uninstalled all tooths.
```

### List all tooths

```shell
> lip list
Tooth                            Version
-------------------------------- ----------
github.com/liteldev/exampletooth 1.0.0
```

### Show information of a tooth

```shell
> lip show github.com/liteldev/exampletooth
Tooth-path: github.com/liteldev/exampletooth
Version: 1.0.0
Name: Example Tooth
Description: An example tooth
Author: Example User
License: MIT
Homepage: www.example.com
```

## Next Steps

You can read pages under Commands directory to get more detailed descriptions of Lip commands.