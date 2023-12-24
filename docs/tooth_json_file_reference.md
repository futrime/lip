# tooth.json File Reference

Each tooth is defined by a tooth.json file that describes the tooth's properties, including its dependencies on other teeth and other information.

You can generate a tooth.json file by running the lip tooth init command. The following example creates a tooth.json file:

```shell
lip tooth init
```

## Schema

Refer to <https://github.com/lippkg/lip/blob/main/schemas/tooth.v2.schema.json>.

## Example

A tooth.json includes directives as shown in the following example. These are described elsewhere in this topic.

```json
{
    "format_version": 2,
    "tooth": "github.com/tooth-hub/example",
    "version": "1.0.0",
    "info": {
        "name": "Example",
        "description": "An example package",
        "author": "exmaple",
        "tags": [
            "example"
        ]
    },
    "commands": {
        "pre_install": [
            "echo \"pre_install\""
        ],
        "post_install": [
            "echo \"post_install\""
        ],
        "pre_uninstall": [
            "echo \"pre_uninstall\""
        ],
        "post_uninstall": [
            "echo \"post_uninstall\""
        ]
    },
    "dependencies": {
        "github.com/tooth-hub/example-deps": ">=1.0.0 <2.0.0 || >=3.0.0 <4.0.0"
    },
    "prerequisites": {
        "github.com/tooth-hub/example-pre": ">=1.0.0"
    },
    "files": {
        "place": [
            {
                "src": "example.js",
                "dest": "dir/example.js"
            },
            {
                "src": "plug/*",
                "dest": "dir/plug/"
            }
        ],
        "preserve": [
            "dir/data.json"
        ],
        "remove": [
            "dir/temp.txt"
        ]
    }
}
```

## `format_version` (required)

Indicates the format of the tooth.json file. lip will parse tooth.json according to this field.

### Examples

```json
{
    "format_version": 2
}
```

### Notes

You should set the format_version to 2.

## `tooth` (required)

Declares the tooth's tooth repository path, which is the tooth's unique identifier (when combined with the tooth version number).

### Syntax

Generally, tooth path should be in the form of a URL without protocol prefix (e.g. github.com/tooth-hub/corepack).

Only letters, digits, dashes, underlines, dots and slashes [a-z0-9-_./] are allowed. Uppercase letters will be converted to lowercase before parsing.

### Examples

```json
{
    "tooth": "github.com/tooth-hub/mytooth"
}
```

### Notes

The tooth path must uniquely identify your tooth. For most teeth, the path is a URL where lip can find the code. For teeth that won’t ever be downloaded directly, the tooth path can be just some name you control that will ensure uniqueness.

Note that the tooth path should not include protocol prefix (e.g. "https://" or "git://"), which already violates the syntax. Meanwhile, the tooth path should not end with ".tth", which will be regarded as a standalone tooth archive file.

If you would like to publish your tooth, please make the tooth path a real URL. For example, the first character should be a letter or a digit.

## `version` (required)

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

When releasing your tooth, you should set the Git tag with prefix "v", e.g. v1.2.3. Otherwise, lip will not correctly parse the tags.

Since GOPROXY regards versions with prefix "v0.0.0" as psuedo-versions, you should not set the version beginning with "0.0.0" if you would like to publish your tooth.

## `info` (required)

Declares necessary information of your tooth.

### Syntax

Provide the name, description , author and tags of your tooth. Every field is required.

### Examples

```json
{
    "info": {
        "name": "Example",
        "description": "An example package",
        "author": "example",
        "tags": [
            "example"
        ]
    }
}
```

## `commands` (optional)

Declare commands to run before or after installing or uninstalling the tooth.

### Syntax

This field contains four sub-fields:

- `pre-install`: an array of commands to run before installing the tooth. (optional)
- `post-install`: an array of commands to run after installing the tooth. (optional)
- `pre-uninstall`: an array of commands to run before uninstalling the tooth. (optional)
- `post-uninstall`: an array of commands to run after uninstalling the tooth. (optional)

Each item in the array is a string of the command to run. The command will be run in the workspace.

### Examples

```json
{
    "commands": {
        "pre-install": [
            "echo Pre-install command"
        ],
        "post-install": [
            "echo Post-install command"
        ],
        "pre-uninstall": [
            "echo Pre-uninstall command"
        ],
        "post-uninstall": [
            "echo Post-uninstall command"
        ]
    }
}
```

## `dependencies` (optional)

Declare dependencies of your tooth.

### Syntax

The key of each field is the tooth repository path of the dependency. The value is the version matching rule of the dependency.

lip provides some version matching rules:

- **1.2.0** Must match 1.2.0 exactly
- **>1.2.0** Must be greater than 1.2.0 but keeping the major version, e.g. 1.3.0, 1.4.0, etc., but not 2.0.0
- **>=1.2.0** etc.
- **<1.2.0**
- **<=1.2.0**
- **!1.2.0** Must not be 1.2.0
- **1.2.x** 1.2.0, 1.2.1, etc., but not 1.3.0

All rules in the outermost list will be calculated with OR, and rules in nested lists will be calculated with AND. In the following example, `github.com/tooth-hub/example-deps` can match version 1.0.0, 1.0.6, 1.1.0 and 2.0.9 but not 1.2.0 and you can regard its rule as:

```
(>=1.0.0 AND <=1.1.0) OR 2.0.x
```

### Examples

```json
{
    "dependencies": {
        "github.com/tooth-hub/example-deps": ">=1.0.0 <=1.1.0 || 2.0.x"
    }
}
```

## `prerequisites` (optional)

Declare prerequisites of your tooth. The syntax follows the `dependencies` field. The key difference is that prerequisites will not be installed by lip automatically.

### Notes

Some teeth should not be installed automatically, e.g. bds. Automatically installing these teeth may cause severe imcompatibility issues.

## `files` (optional)

Describe how the files in your tooth should be handled.

### Syntax

This field contains three sub-fields:

- `place`: an array to specify how files in the tooth should be place to the workspace. Each item is an object with three sub-fields: (optional)
  - `src`: the source path of the file. It can be a file or a directory with suffix "*" (e.g. `plug/*`). (required)
  - `dest`: the destination path of the file. It can be a file or a directory. If `src` has suffix "*", `dest` must be a directory. Otherwise, `dest` must be a file. (required)
- `preserve`: an array to specify which files in `place` field should be preserved when uninstalling the tooth. Each item is a string of the path of the file. (optional)
- `remove`: an array to specify which files should be removed when uninstalling the tooth. Each item is a string of the path of the file. (optional)

### Examples

```json
{
    "files": {
        "place": [
            {
                "src": "plug/*",
                "dest": "plugins"
            },
            {
                "src": "config.yml",
                "dest": "config.yml"
            }
        ],
        "preserve": [
            "config.yml"
        ],
        "remove": [
            "plugins/ExamplePlugin.dll"
        ]
    }
}
```

### Notes

- Files specified in `place` but not in `preserve` will be removed when uninstalling the tooth. Therefore, you don't need to specify them in `remove`.
- `remove` field is prior to `preserve` field. If a file is specified in both fields, it will be removed.
- Only `place` filed support "*" suffix. `preserve` and `remove` fields do not support it.

## `platforms` (optional)

Declare platform-specific configurations.

### Syntax

This field is an array of platform-specific configurations. Each item is an object with these sub-fields:

- `commands`: same as `commands` field. (optional)
- `dependencies`: same as `dependencies` field. (optional)
- `files`: same as `files` field. (optional)
- `goos`: the target operating system. For the values, see [here](https://go.dev/doc/install/source#environment). (required)
- `goarch`: the target architecture. For the values, see [here](https://go.dev/doc/install/source#environment). Omitting means match all. (optional)

If provided and matched, the platform-specific configuration will override the global configuration.

### Examples

```json
{
    "platforms": [
        {
            "commands": {
                "pre-install": [
                    "echo Pre-install command for Windows"
                ]
            },
            "dependencies": {
                "github.com/tooth-hub/example-deps": ">=1.0.0 <=1.1.0 || 2.0.x"
            },
            "files": {
                "place": [
                    {
                        "src": "plug/*",
                        "dest": "plugins"
                    },
                    {
                        "src": "config.yml",
                        "dest": "config.yml"
                    }
                ],
                "preserve": [
                    "config.yml"
                ],
                "remove": [
                    "plugins/ExamplePlugin.dll"
                ]
            },
            "goos": "windows"
        },
        {
            "commands": {
                "pre-install": [
                    "echo Pre-install command for Linux AMD64"
                ]
            },
            "dependencies": {
                "github.com/tooth-hub/example-deps": ">=1.0.0 <=1.1.0 || 2.0.x"
            },
            "files": {
                "place": [
                    {
                        "src": "plug/*",
                        "dest": "plugins"
                    },
                    {
                        "src": "config.yml",
                        "dest": "config.yml"
                    }
                ],
                "preserve": [
                    "config.yml"
                ],
                "remove": [
                    "plugins/ExamplePlugin.dll"
                ]
            },
            "goos": "linux",
            "goarch": "amd64"
        }
    ]
}
```

### Notes

If multiple platform-specific configurations are matched, the last one will override the previous ones. Therefore, you should put the most specific configuration at the end of the array.

If a platform-specific configuration is set, `commands`, `dependencies` and `files` in the global configuration will be ignored, no matter whether they are set or not in the platform-specific configuration. Thus, it is highly recommended not to set any of them in the global configuration if you would like to set platform-specific configurations.
