# Getting Started

To start using lip, you'll want to [install lip](./installation.md) on your system.

## Check Your lip is Working

First things first, open your terminal and run this command to make sure lip is installed correctly:

```bash
$ lip --version
0.26.0
```

If that looks good, you're all set—lip is working like a charm.

If the output seems off, swing by the [Installation](./installation.md) page for a chill, step-by-step guide to get lip up and running on Windows, macOS, or Linux.

## Common Tasks

### Install a Package

```bash
$ lip install example.com/pkg@1.0.0
```

By default, lip grabs packages via [goproxy.io](https://goproxy.io). If you prefer fetching packages directly using Git, simply clear the proxy list with:
  
```bash
lip config set go_module_proxies=
```

Or, set a custom proxy with:

```bash
lip config set go_module_proxies=https://proxy.example.com
```

### Install a Package from a Local Directory

```bash
$ lip install /path/to/pkg/
```

lip will detect the tooth.json file in that directory and install the package.

### Install a Package from a Local Archive

```bash
$ lip install /path/to/pkg.tar.gz
```

### Install Multiple Packages Using a tooth.json File

Create a tooth.json file in your current directory and run `lip install` to install multiple packages at once—an awesome way to streamline things.

### Updating a Package

```bash
$ lip update example.com/pkg@1.0.0
```

Remember to always specify the version when updating a package. lip doesn't support updating to the latest version automatically because the package sources aren't fully synced.

### Uninstall a Package

```bash
$ lip uninstall example.com/pkg
```

Avoid specifying a version when uninstalling a package.
