# Quickstart

!!! tip
    Not accustomed to using command-line tools? You can use [LipUI](https://github.com/lippkg/LipUI).

To get started with using lip, you should [have lip installed](install.md) first. If you are not familiar with the command-line interface, you can also use [lipUI](https://github.com/lippkg/LipUI) to manage your teeth.

## Ensure you have a working lip

As a first step, you should check that you have a working lip installed. This can be done by running the following commands and making sure that the output looks similar.

```shell
> lip --version
lip 0.15.0 from C:\Users\ExampleUser\AppData\Local\lip\lip.exe
```

## Common Tasks

### Install a tooth

```shell
> lip install github.com/tooth-hub/bdsdownloader
[...]
Done.
```

By default, lip will fetch teeth via GOPROXY, a proxy of Git repos.

### Install a tooth from a tooth file

```shell
> lip install ./bdsdownloader.tth
[...]
Done.
```

### Install multiple teeth

lip suppports installing multiple files at a time.

```shell
> lip install github.com/liteldev/bdsdownloader github.com/tooth-hub/crashlogger
[...]
Done.
```

### Upgrade a tooth

```shell
> lip install --upgrade github.com/liteldev/bdsdownloader
[...]
Done.
```

### Uninstall a tooth

To uninstall a tooth, you must provide the tooth path of the tooth.

```shell
> lip uninstall github.com/liteldev/bdsdownloader
[...]
Done.
```

### List all teeth

```shell
> lip list
+------------------------------------+----------------+---------+
|               TOOTH                |      NAME      | VERSION |
+------------------------------------+----------------+---------+
| github.com/tooth-hub/bdsdownloader | BDS Donwloader | 0.3.1   |
| github.com/tooth-hub/crashlogger   | CrashLogger    | 1.0.1   |
| github.com/tooth-hub/peeditor      | PeEditor       | 3.2.0   |
+------------------------------------+----------------+---------+
```

### Show information of a tooth

```shell
> lip show github.com/liteldev/exampletooth
+-------------+------------------------------------+
|     KEY     |               VALUE                |
+-------------+------------------------------------+
| Tooth Repo  | github.com/tooth-hub/bdsdownloader |
| Name        | BDS Donwloader                     |
| Description | A CLI tool to download BDS         |
| Author      | Jasonzyt                           |
| Version     | 0.3.1                              |
+-------------+------------------------------------+
```

## Next Steps

You can read pages under Commands directory to get more detailed descriptions of lip commands. If you are a creator and want to publish your tooth, you can read [this tutorial](tutorials/create_a_lip_tooth.md) to learn how to do it.
