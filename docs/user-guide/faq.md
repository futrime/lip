# FAQ

## How to clear the cache?

```bash
$ lip cache clean
```

## How to update/uninstall lip?

So far, lip installer has not been implemented. You can update/uninstall lip manually by replacing/removing the binary in your PATH with the latest release. If you are uninstalling lip, you can also remove the PATH configuration.

## How to create a pacakge  

First, run the following command to initialize the current directory as a package:

```bash
$ lip init
```

Then, edit the `tooth.json` file to define the package. Refer to [the tooth.json reference](./files/tooth-json.md) for more information.

To test the installation of the package (be aware of the interference with the existing files since the package is installed in the current directory), run:

```bash
$ lip install
```

To pack the package, run:

```bash
$ lip pack my-package.zip
```
