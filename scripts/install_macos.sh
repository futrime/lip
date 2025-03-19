#!/usr/bin/env sh

set -e

# Generate the platform suffix based on the architecture.
ARCH=$(arch)
if [ "$ARCH" = "arm64" ]; then
  PLATFORM_SUFFIX="arm64"
elif [ "$ARCH" = "x86_64" ]; then
  PLATFORM_SUFFIX="x64"
else
    echo "Unsupported architecture: $ARCH"
    exit 1
fi

# Build the download URL.
if [ -z "$GITHUB_MIRROR" ]; then
  GITHUB_MIRROR="https://github.com"
fi
GITHUB_MIRROR=$(echo "$GITHUB_MIRROR" | sed 's:/*$::')
DOWNLOAD_URL="$GITHUB_MIRROR/futrime/lip/releases/latest/download/lip-cli-osx-$PLATFORM_SUFFIX.zip"

# Download the release.
TEMP_DIR=$(mktemp -d)
echo "Downloading $DOWNLOAD_URL"
curl -fLS $DOWNLOAD_URL | tar -x -C $TEMP_DIR

# Install the binary.
INSTALL_DIR_FROM_HOME=".local/bin"
INSTALL_DIR="$HOME/$INSTALL_DIR_FROM_HOME"
echo "Installing to $INSTALL_DIR"
if [ ! -d "$INSTALL_DIR" ]; then
  mkdir -p "$INSTALL_DIR"
fi
chmod 755 "$TEMP_DIR/lip"
mv "$TEMP_DIR/lip" "$INSTALL_DIR"
rm -rf $TEMP_DIR

# Hint the user to add the binary to PATH.
echo "Please add $INSTALL_DIR to PATH to use the 'lip' command."
echo "You can do this by adding the following line to your shell profile (~/.zprofile, etc):"
echo
echo "PATH=\"\$HOME/$INSTALL_DIR_FROM_HOME:\$PATH\""
echo
