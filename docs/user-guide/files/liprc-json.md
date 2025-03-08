# liprc.json

The `liprc.json` file serves as the configuration file for lip, enabling you to configure settings such as caching, proxies, and script execution. lip stores this file at `%APPDATA%\lip\liprc.json` for Windows, `~/.config/lip/liprc.json` for Linux, and `~/Library/Application Support/lip/liprc.json` for macOS.

Project-specific settings take precedence over user-wide settings. All configuration fields are optional.

## Configuration Fields

### cache

- Type: `string`
- Default: `%LocalAppData%\lip\cache` for Windows, `~/.local/share/lip/cache` for Linux, and `~/Library/Application Support/lip/cache` for macOS

Defines the directory path for storing cached files.

### github_proxies

- Type: `string`
- Default: "" (empty string)

Sets proxy URLs (separated by commas) for GitHub connections. When left empty, lip connects to GitHub directly.

### go_module_proxies

- Type: `string`
- Default: `https://goproxy.io`

Defines Go module proxy URLs (separated by commas) for Go module downloads.
