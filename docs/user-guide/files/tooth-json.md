# tooth.json

The `tooth.json` file defines a package's metadata, identifier, version, dependencies, files, and other configuration settings. This file must be in JSON format and located in the package's root directory.

For the complete JSON schema specification, see [tooth.v3.schema.json](../../schemas/tooth.v3.schema.json).

In the documentation below, fields are marked as either (required) or (optional). Note that if a parent field is optional but contains required child fields, those child fields become mandatory only if the parent field is included. For example, while `variants` is optional, if you include it, each variant must specify a `platform`.

Scriban expressions allow you to dynamically reference other fields within any string value, making package maintenance easier. For example, use `{{tooth}}` to reference the package's tooth path or `{{version}}` to access the package's version number. This dynamic referencing helps keep your package configurations DRY (Don't Repeat Yourself) and more maintainable. However, some package manifest manipulation commands (e.g. commands with `--save` option) may not support Scriban expressions.

## Fields

### format_version (required)

The format version number. Currently only version `3` is supported.

### format_uuid (required)

The format's unique identifier. Currently only `289f771f-2c9a-4d73-9f3f-8492495a924d` is supported.

### tooth (required)

The package's tooth path, a unique identifier in Go module path format (a URL without scheme and suffix).

Examples:

- `github.com/LiteLDev/LeviLamina`

Note: To publish a package, its tooth path must match the repository URL where it's hosted.

### version (required)

The package version in [semantic versioning](https://semver.org) format.

Examples:

- `1.0.0`
- `1.0.0-alpha.1`
- `0.1.0`

When creating release tags, prefix versions with `v` (e.g., `v1.0.0`). However, omit the `v` prefix in the `version` field. Avoid using `v0.0.0` as Go module proxy treats it as a pseudo-version.

### info (optional)

Package metadata fields.

### info.name (optional)

The display name of the package.

### info.description (optional)

A brief description of the package.

### info.tags (optional)

Package tags in either of these formats:

- Simple tag: `tag`
- Key-value pair: `tag:subtag`

Tags and subtags may only contain lowercase letters, numbers, and hyphens ([a-z0-9-]). While lip treats both formats equally, some platforms like [Bedrinth](https://bedrinth.com) may use them differently.

### info.avatar_url (optional)

URL to the package's avatar/icon image.

### variants (optional)

An array of platform-specific package configurations. This is the core configuration section of `tooth.json`.

lip processes variants in order, applying all that match the current platform. When multiple variants match, their configurations are merged.

Note: For platform compatibility checks, lip ignores variants using glob patterns. To support multiple platforms, define separate variants for each, even if they're empty.

### variants[].label (optional)

The label for this variant. Users can install a specific variant by label with `lip install <tooth>#<label>@<version>`. Should either match `^[a-z0-9]+(_[a-z0-9]+)*$ or be a glob pattern. If omitted or empty, the variant is considered the default.

For a variant label to be recognized, there must be at least one variant with a non-globbed label defined in the variants array. Glob patterns in label fields only take effect when their corresponding non-globbed labels are also defined. For example:

- Having only `label_*` does not indicate support for any label
- To support `label_a` and `label_b`, you need either:
  - Two separate variants with exact labels
  - One variant with exact label and another with `label_*`

### variants[].platform (optional)

The target platform for this variant. Valid values:

- `linux-arm64`
- `linux-x64`
- `osx-arm64`
- `osx-x64`
- `win-arm64`
- `win-x64`
- Glob patterns (e.g., `linux-*`)

For platform variant to be recognized, there must be at least one non-globbed platform variant defined in the variants array. Glob patterns in platform fields only take effect when their corresponding non-globbed platforms are also defined. For example:

- Having only `linux-*` does not indicate support for any Linux platform
- To support `linux-x64` and `linux-arm64`, you need either:
  - Two separate variants with exact platforms
  - One variant with exact platform and another with `linux-*`

If omitted or empty, the variant is considered platform-agnostic, i.e.. glob pattern `*`.

### variants[].dependencies (optional)

Package dependencies, specified as key-value pairs. Keys are package identifiers (optionally with subdirectory paths), and values are version constraints.

Examples:

Keys:

- `github.com/futrime/example-package`
- `github.com/futrime/example-package#subpath`

Version constraints:

- Exact: `1.0.0`
- Range: `>=0.1.0 <1.0.0`

Version parsing uses [WalkerCodeRanger/semver](https://github.com/WalkerCodeRanger/semver).

### variants[].assets (optional)

Defines how package files should be handled.

### variants[].assets[].type (required)

The asset type:

- `self`: Files from the package itself
- `tar`: TAR archive
- `tgz`: Gzipped TAR archive
- `uncompressed`: Single uncompressed file
- `zip`: ZIP archive

### variants[].assets[].urls (optional)

Download URLs for the asset, tried in order. For `self` type assets, this should be empty.

### variants[].assets[].place (optional)

Rules for placing files in the workspace.

### variants[].assets[].place[].type (required)

Placement type:

- `file`: Single file placement
- `dir`: Directory placement

Note: `uncompressed` assets only support `file` type placement.

### variants[].assets[].place[].src (required)

Source path specification:

- For `uncompressed` assets: Leave empty (`""`)
- For `file` type: File path or glob pattern (matched directories are ignored, files are flattened)
- For `dir` type: Directory path (preserves structure)

### variants[].assets[].place[].dest (required)

Destination path:

- For file placement: Target file path
- For directory/glob placement: Target directory path

### variants[].assets[].preserve (optional)

Paths or glob patterns for files to keep during uninstallation.

### variants[].assets[].remove (optional)

Paths or glob patterns for files to remove during uninstallation. This will override `preserve` settings.

### variants[].scripts (optional)

Commands to execute in the workspace. Define as key-value pairs where keys are script names and values are commands. If more than matched variants define the same script, only the last one is used.

Built-in script hooks:

- `pre_install`: Before installation
- `install`: After file placement
- `post_install`: After installation
- `pre_pack`: Before packaging (only applies to default variant)
- `post_pack`: After packaging (only applies to default variant)
- `pre_uninstall`: Before uninstallation
- `uninstall`: After file removal
- `post_uninstall`: After uninstallation

Custom scripts can be run using `lip run <script>`. Custom script names should match`^[a-z0-9]+(_[a-z0-9]+)*$`.
