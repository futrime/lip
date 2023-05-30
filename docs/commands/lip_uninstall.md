# lip uninstall

## Usage

```shell
lip uninstall [options] <tooth paths>

aliases: remove, rm, un, r
```

## Description

Uninstall tooths.
This command will remove the files released by the tooth package and the contents of the folder that the tooth author specified the tooth to occupy.

## Options

- `-h, --help`

  Show help.

- `-y, --yes`

  Skip the confirmation prompt.

- `--keep-possession`

  Keep files that the tooth author specified the tooth to occupy. These files are often configuration files, data files, etc.