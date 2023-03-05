# lip exec

## Usage

```shell
lip exec [options] <tool> [args...]

alias: x
```

## Description

Execute a Lip tool. Tools should be installed with `lip install` first.

In fact, this will execute ./lip/tools/\<tool>/\<tool> (or .\lip\tools\\\<tool>\\\<tool>.exe or .\lip\tools\\\<tool>\\\<tool>.cmd on Windows).

## Options

- `-h, --help`

  Show help.

## Examples

You can even execute Lip itself:

```shell
lip exec lip list
```