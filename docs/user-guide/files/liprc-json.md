# liprc.json

The `liprc.json` file serves as the configuration file for lip, enabling you to configure settings such as caching, proxies, and script execution. lip stores this file at `%APPDATA%\lip\liprc.json` for Windows and `~/.config/lip/liprc.json` for POSIX-like systems.

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

### github_proxy

- Type: `string`
- Default: "" (empty string)

Sets proxy URLs (separated by commas) for GitHub connections. When left empty, lip connects to GitHub directly.

### go_module_proxy

- Type: `string`
- Default: `https://proxy.golang.org`

Defines Go module proxy URLs (separated by commas) for Go module downloads.

### https_proxy

- Type: `string`
- Default: "" (empty string)

Sets the HTTPS proxy URL. Environment variables (`HTTPS_PROXY`, `https_proxy`, `HTTP_PROXY`, or `http_proxy`) will take precedence if set.

### noproxy

- Type: `string`
- Default: "" (empty string)

Lists domains that should bypass the proxy. The `no_proxy` and `NO_PROXY` environment variable will override this setting if set.

### proxy

- Type: `string`
- Default: "" (empty string)

Defines a general proxy URL for all connections. The `HTTP_PROXY` or `http_proxy` environment variable will take precedence if set.
