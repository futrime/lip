# runtime_config.json

The `runtime_config.json` file serves as the configuration file for lip, enabling you to configure settings such as caching, proxies, and script execution. lip stores this file at `%APPDATA%\lip\runtime_config.json` for Windows and `~/.config/lip/runtime_config.json` for POSIX-like systems.

Project-specific settings take precedence over user-wide settings. All configuration fields are optional.

## Configuration Fields

### cache

- Type: `string`
- Default: `%LocalAppData%\lip\cache` (Windows) or `~/.local/share/lip/cache` (POSIX-like systems)

Defines the directory path for storing cached files.

### color

- Type: `boolean`
- Default: `true`

Controls colored output in the terminal. Note: The `NO_COLOR` environment variable, if set, will override this setting.

### git

- Type: `string`
- Default: `git`

Specifies the path to your Git executable.

### github_proxy

- Type: `string`
- Default: "" (empty string)

Sets a proxy URL for GitHub connections. When left empty, lip connects to GitHub directly.

### go_module_proxy

- Type: `string`
- Default: `https://goproxy.io`

Defines the proxy URL for Go module downloads.

### https_proxy

- Type: `string`
- Default: "" (empty string)

Sets the HTTPS proxy URL. Environment variables (`HTTPS_PROXY`, `https_proxy`, `HTTP_PROXY`, or `http_proxy`) will take precedence if set.

### noproxy

- Type: `string`
- Default: "" (empty string)

Lists domains that should bypass the proxy. The `NO_PROXY` environment variable will override this setting if set.

### proxy

- Type: `string`
- Default: "" (empty string)

Defines a general proxy URL for all connections. The `HTTP_PROXY` or `http_proxy` environment variable will take precedence if set.

### script_shell

- Type: `string`
- Default: `cmd.exe` (Windows) or `/bin/sh` (POSIX-like systems)

Specifies which shell to use when executing scripts.
