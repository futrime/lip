# lip init

Create a tooth.json file

## Usage

```shell
lip init
```

## Description

This command can be used to set up a new or existing lip package.

lip will ask a bunch of questions, and then write a tooth.json for you. You can also use `-y` / `--yes` to skip the questionnaire altogether, and the values will be set to empty strings. You can also use the `--init-*` options to set the values directly and skip the corresponding questions.

## Examples

Basic initialization:

```shell
# Create a new project using the questionnaire
lip init

# Create a new project using all default values
lip init -y
```

Initialization with specific values:

```shell
# Create a new project with initial values
lip init --init-name "my-mod" \
    --init-description "A cool LeviLamina mod" \
    --init-tooth "github.com/developer/my-mod" \
    --init-version "0.1.0"
```

## Options

- `-f, --force`

  Overwrite the existing tooth.json file.

- `--init-avatar-url <avatar-url>`

  The avatar URL to use.

- `--init-description <description>`

  The description to use.

- `--init-name <name>`

  The name to use.

- `--init-tooth <tooth-path>`

  The package's tooth path to use.

- `--init-version <version>`

  The version to use.

- `-y, --yes`

  Skip confirmation prompts.
