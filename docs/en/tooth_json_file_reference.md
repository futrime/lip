# tooth.json File Reference

Each Lip tooth is defined by a tooth.json file that describes the tooth's properties, including its dependencies on other tooths and other information.

These properties include:

- The **format version** of the tooth.json file.

- The current tooth's **tooth path**. This should be a location where the tooth can be downloaded by Lip, such as the tooth code's Git repository location. This serves as a unique identifier, when combined with the tooth’s version number.

- The current tooth's **version**.

- **Dependencies** along with there versions required by the current tooth.

- The current tooth's **information**, including the name, the author, the description and so on.

- The current tooth's **placement**. This is a list of files that should be placed in the tooth's installation directory.

- The current tooth's **possession**. This is a list of files that should be placed in the tooth's possession directory.

The **format_version**, **tooth path** and **version** are required. The other properties are optional.

You can generate a tooth.json file by running the lip tooth init command. The following example creates a tooth.json file:

```shell
lip tooth init
```

## Example

A tooth.json includes directives as shown in the following example. These are described elsewhere in this topic.

```json
{
  "format_version": 1,
  "tooth": "github.com/liteldev/liteloaderbds",
  "version": "2.9.0",
  "dependencies": {
    "test.test/test/depend": [
      [
        ">=1.0.0",
        "<=1.1.0"
      ],
      [
        "2.0.x"
      ]
    ]
  },
  "information": {
    "name": "LiteLoaderBDS",
    "description": "Epoch-making and cross-language Bedrock Dedicated Server plugin loader.",
    "author": "LiteLDev",
    "license": "Modified LGPL-3.0",
    "homepage": "www.litebds.com"
  },
  "placement": [
    {
      "source": "LiteLoader.dll",
      "destination": "LiteLoader.dll"
    }
  ],
  "possession": [
    "plugins/LiteLoader/"
  ],
  "commands": [
    {
      "type": "install",
      "commands": [
        "start LLPeEditor.exe"
      ],
      "GOOS": "windows"
    }
  ]
}
```

## format_version

Indicates the format of the tooth.json file. Lip will parse tooth.json according to this field.

### Examples

```json
{
  "format_version": 1
}
```

### Notes

Now only 1 is a legal value.

## tooth

Declares the tooth's tooth path, which is the tooth's unique identifier (when combined with the tooth version number).

### Syntax

Generally, tooth path should be in the form of a lowercased URL without protocol prefix (e.g. github.com/liteldev/liteloaderbds).

Only lowercase letters, digits, dashes, underlines, dots and slashes [a-z0-9-_./] are allowed. Uppercase letters will be converted to lowercase before parsing.

### Examples

```json
{
  "tooth": "example.com/mytooth"
}
```

### Notes

The tooth path must uniquely identify your tooth. For most tooths, the path is a URL where Lip can find the code. For tooths that won’t ever be downloaded directly, the tooth path can be just some name you control that will ensure uniqueness.

Note that the tooth path should not include protocol prefix (e.g. "https://" or "git://"), which already violates the syntax. Meanwhile, the tooth path should not end with ".tth", which will be regarded as a standalone tooth archive file.

If you would like to publish your tooth, please make the tooth path a real URL. For example, the first character should be a letter or a digit.

## version

### Syntax

We adopted [Semantic Versioning 2.0.0](https://semver.org) and simplified its rules.

- A normal version number MUST take the form X.Y.Z where X, Y, and Z are non-negative integers, and MUST NOT contain leading zeroes, e.g. 1.01.02 is forbidden. X is the major version, Y is the minor version, and Z is the patch version. Each element MUST increase numerically. For instance: 1.9.0 -> 1.10.0 -> 1.11.0.

- Once a versioned tooth has been released, the contents of that version MUST NOT be modified. Any modifications MUST be released as a new version.

- Major version zero (0.y.z) is for initial development. Anything MAY change at any time. The public API SHOULD NOT be considered stable. When under early development, please set the major version to zero.

- Patch version Z (x.y.Z | x > 0) MUST be incremented if only backwards compatible bug fixes are introduced. A bug fix is defined as an internal change that fixes incorrect behavior.

- Minor version Y (x.Y.z | x > 0) MUST be incremented if new, backwards compatible functionality is introduced to the public API. It MUST be incremented if any public API functionality is marked as deprecated. It MAY be incremented if substantial new functionality or improvements are introduced within the private code. It MAY include patch level changes. Patch version MUST be reset to 0 when minor version is incremented.

- Major version X (X.y.z | X > 0) MUST be incremented if any backwards incompatible changes are introduced to the public API. It MAY also include minor and patch level changes. Patch and minor versions MUST be reset to 0 when major version is incremented.

- A pre-release version MAY be denoted by appending a hyphen and up to two dot separated identifiers immediately following the patch version. The first identifier MUST comprise only lowercase letters [a-z] and the second identifier (if used) MUST comprise only numbers. Identifiers MUST NOT be empty. Numeric identifiers MUST NOT include leading zeroes. Pre-release versions have a lower precedence than the associated normal version and their patch versions MUST be zero. A pre-release version indicates that the version is unstable and might not satisfy the intended compatibility requirements as denoted by its associated normal version. Examples: 1.0.0-alpha, 1.0.0-alpha.1, 1.2.0-beta. Note that 1.0.1-alpha is not allowed.

- Precedence refers to how versions are compared to each other when ordered. It is calculated according to the following rules:

  1. Precedence MUST be calculated by separating the version into major, minor, patch and pre-release identifiers in that order.

  2. Precedence is determined by the first difference when comparing each of these identifiers from left to right as follows: Major, minor, and patch versions are always compared numerically.

   Example: 1.0.0 < 2.0.0 < 2.1.0 < 2.1.1.

  3. When major, minor, and patch are equal, a pre-release version has lower precedence than a normal version.

   Example: 1.0.0-alpha < 1.0.0.

  4. Precedence for two pre-release versions with the same major, minor, and patch version MUST be determined by comparing each dot separated identifier from left to right until a difference is found as follows:

   1. Identifiers consisting of only digits are compared numerically.

   2. Identifiers with letters or hyphens are compared lexically in ASCII sort order. When one of the two identifiers has reached its end but another has not, it will has a lower precedence.

   Example: 1.0.0-alph < 1.0.0-alpha < 1.0.0-alpha.1 < 1.0.0-beta < 1.0.0-beta.2 < 1.0.0-beta.11 < 1.0.0-rc.1 < 1.0.0.

### Examples

Example of a production release:

```json
{
  "version": "1.2.3"
}
```

Example of a pre-release:

```json
{
  "version": "1.2.0-beta.3"
}
```

Example of a early development release:

```json
{
  "version": "0.1.2"
}
```

### Notes

When releasing your tooth, you should set the Git tag with prefix "v", e.g. v1.2.3. Otherwise, Lip will not correctly parse the tags.

Since GOPROXY regards versions with prefix "v0.0.0" as psuedo-versions, you should not set the version beginning with "0.0.0" if you would like to publish your tooth.

## dependencies

### Syntax

Lip provides some version matching rules:

- **1.2.0** Must match 1.2.0 exactly
- **>1.2.0** Must be greater than 1.2.0 but keeping the major version, e.g. 1.3.0, 1.4.0, etc., but not 2.0.0
- **>=1.2.0** etc.
- **<1.2.0**
- **<=1.2.0**
- **!1.2.0** Must not be 1.2.0
- **1.2.x** 1.2.0, 1.2.1, etc., but not 1.3.0

All rules in the outermost list will be calculated with OR, and rules in nested lists will be calculated with AND. In the following example, test.test/test/depend can match version 1.0.0, 1.0.6, 1.1.0 and 2.0.9 but not 1.2.0 and you can regard its rule as:

```
(>=1.0.0 AND <=1.1.0) OR 2.0.x
```

Multi-level nesting is not allowed.

### Examples

```json
{
  "dependencies": {
    "test.test/test/depend": [
      [
        ">=1.0.0",
        "<=1.1.0"
      ],
      [
        "2.0.x"
      ]
    ]
  }
}
```

### Notes

Minor version wildcard is not allowed, e.g. you cannot use 1.x.x.

## information

Declares necessary information of your tooth, and add any information as you like.

### Syntax

This field has no syntax restriction. You can write anything following JSON rules.

### Examples

```json
{
  "information": {
    "name": "LiteLoaderBDS",
    "description": "Epoch-making and cross-language Bedrock Dedicated Server plugin loader.",
    "author": "LiteLDev",
    "license": "Modified LGPL-3.0",
    "homepage": "www.litebds.com",
    "thanks": "All contributors!"
  }
}
```

### Notes

Some fields are customary and might be shown on the search pages of some registries. These fields are listed below:

- name: the name of the tooth
- description: a line of brief description of the tooth
- author: your name
- license: the license of the tooth, left empty if private
- homepage: the homepage of the tooth

## placement

Indicates how should Lip handle file placement. When installing, files from "source" will be placed to "destination". When uninstalling, files at "destination" will be removed.

### Syntax

Each placement rule should contain a source field and a destination field. Lip will extract files from the path relative to the root of the tooth specified by source and place them to the path specified by destination.

If both the source and the destination ends with "*", the placement will be regarded as a wildcard. Lip will recursively place all files under the source directory to the destination directory.

You can also specify GOOS and GOARCH to optionally place files for specific platforms. For example, you can specify "windows" and "amd64" to place files only for Windows 64-bit. If you want to place files for all platforms, you can omit the GOOS and GOARCH fields. However, if you have specified GOARCH, you must also specify GOOS.

### Examples

Extract from specific folders and place to specific folders:

```json
{
  "placement": [
    {
      "source": "build",
      "destination": "plugins"
    },
    {
      "source": "assets",
      "destination": "plugins/myplugin",
      "GOOS": "windows"
    },
    {
      "source": "config",
      "destination": "plugins/myplugin/config",
      "GOOS": "windows",
      "GOARCH": "amd64"
    }
  ]
}
```

## possession

Declares the which folders are in the possession of the tooth. When uninstalling, files in the declared folders will be removed. However, when upgrading or reinstalling, Lip will keep files in both the possession of the previous version and the version to install (but those dedicated in placement will still be removed).

### Syntax

Each item of the list should be a valid directory path ending with "/".

### Examples

```json
{
  "possession": [
    "plugins/LiteLoader/"
  ]
}
```

### Notes

Do not take the possession of any directory that might be used by other tooth, e.g. public directories like worlds/.

## commands

Declares the commands that will be executed when installing.

### Syntax

Each item of the list should be a valid command. Lip will execute the command in the root of BDS.

type is the type of the command. It can be one of the following:

- install: execute the command when installing
- uninstall: execute the command when uninstalling

GOOS is the operating system selector, which should match a possible GOOS variable of Go. GOARCH (optional) is the platform selector, which should match a possible GOARCH variable of Go. If GOARCH is not specified, Lip will execute the command on all platforms.

Available GOOS and GOARCH (in GOOS/GOARCH format):

```
darwin/amd64
darwin/arm64
linux/amd64
linux/arm64
openbsd/amd64
openbsd/arm64
windows/amd64
windows/arm64
```

### Examples

```json
{
  "commands": [
    {
      "type": "install",
      "commands": [
        "start LLPeEditor.exe"
      ],
      "GOOS": "windows",
      "GOARCH": "amd64"
    }
  ]
}
```

## Syntax

This is a JSON schema of tooth.json, describing the syntax of tooth.json.

```json
{
  "$schema": "https://json-schema.org/draft-07/schema",
  "type": "object",
  "additionalProperties": false,
  "required": [
    "format_version",
    "tooth",
    "version"
  ],
  "properties": {
    "format_version": {
      "enum": [1]
    },
    "tooth": {
      "type": "string",
      "pattern": "^[a-zA-Z\\d-_\\.\\/]*$"
    },
    "version": {
      "type": "string",
      "pattern": "^\\d+\\.\\d+\\.(\\d+|0-[a-z]+(\\.[0-9]+)?)$"
    },
    "dependencies": {
      "type": "object",
      "additionalProperties": false,
      "patternProperties": {
        "^[a-zA-Z\\d-_\\.\\/]*$": {
          "type": "array",
          "uniqueItems": true,
          "minItems": 1,
          "additionalItems": false,
          "items": {
            "type": "array",
            "uniqueItems": true,
            "minItems": 1,
            "additionalItems": false,
            "items": {
              "type": "string",
              "pattern": "^((>|>=|<|<=|!)?\\d+\\.\\d+\\.\\d+|\\d+\\.\\d+\\.x)$"
            }
          }
        }
      }
    },
    "information": {
      "type": "object"
    },
    "placement": {
      "type": "array",
      "additionalItems": false,
      "items": {
        "type": "object",
        "additionalProperties": false,
        "required": [
          "source",
          "destination"
        ],
        "properties": {
          "source": {
            "type": "string"
          },
          "destination": {
            "type": "string"
          },
          "GOOS": {
            "type": "string"
          },
          "GOARCH": {
            "type": "string"
          }
        }
      }
    },
    "possession": {
      "type": "array",
      "additionalItems": false,
      "items": {
        "type": "string"
      }
    },
    "commands": {
      "type": "array",
      "items": {
        "type": "object",
        "additionalProperties": false,
        "required": [
          "type",
          "commands",
          "GOOS"
        ],
        "properties": {
          "type": {
            "enum": ["install", "uninstall"]
          },
          "commands": {
            "type": "array",
            "items": {
              "type": "string"
            }
          },
          "GOOS": {
            "type": "string"
          },
          "GOARCH": {
            "type": "string"
          }
        }
      }
    }
  }
}
```