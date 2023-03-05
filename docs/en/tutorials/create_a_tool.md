# Tutorial: Create a Tool

Since 0.5.0, Lip supports tools. Tools are programs that can be executed by Lip. You can use tools to do some things that Lip cannot do, such as installing a BDS server, packing a world, or even install any other utilities by other package managers like npm.

## Prerequisites

- **Some project management experience.** You ought to learn the basic usage of Git, and the basic syntax of JSON in advance.

- **A tool to edit tooth.json** Any text editor you have will work fine. The most popular are VSCode and Vim.

- **A command terminal** Lip works well with both PowerShell and cmd in Windows.

- **Lip command-line tool** You should install Lip in advance. For more information, refer to [Installation](installation.md)

## Prepare tool distributions

A tool is a executable file. On Windows, .cmd file is also supported. The name of the executable file should be the name of the tool. On Windows, the executable file should be tool_name.exe or tool_name.cmd. If a .exe file is not found, Lip will try to find a .cmd file. However, on other platforms, only files exactly matching the tool_name are supported.

Here we will pack npm (on Windows) as a Lip tool. The file structure of npm is:

```
node_modules/
  ...
npm.cmd
```

## Write tooth.json

Lip will regard executable files with tool name as its name (on Windows, ends with .exe or .cmd) under .lip/tools/tool_name/ as Lip tools. Therefore, you should put the executable file under .lip/tools/tool_name/ and name it as the tool name.

You can create a tooth.json like this:

```json
{
    "format_version": 1,
    "tooth": "example.com/exampleuser/exampletool",
    "version": "1.0.0",
    "dependencies": {},
    "information": {
        "name": "Example Tool",
        "description": "An example tool",
        "author": "Example User",
        "license": "MIT",
        "homepage": "example.com"
    },
    "placement": [
        {
            "source": "node_modules/*",
            "destination": ".lip/tools/npm/node_modules/*"
        },
        {
            "source": "npm.cmd",
            "destination": ".lip/tools/npm/npm.cmd"
        }
    ],
    "possession": [
        ".lip/tools/npm/node_modules/"
    ]
}
```

## Test the tool

In addition to the guide in [Create a Lip Tooth](create_a_lip_tooth.md#test-the-tooth), you should also run the command below to test the tool:

```shell
lip exec npm [args]
```