# Build Guide

1. Initialize Go workspace.

    ```bash
    go work init src
    ```

2. Build Lip.

    ```bash
    go build -o build/ github.com/liteldev/lip
    ```

We also provide GitHub Actions workflow to build Lip. You can find it in `.github/workflows/build.yml`.
