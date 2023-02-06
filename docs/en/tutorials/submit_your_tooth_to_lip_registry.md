# Tutorial: Submit Your Tooth to Lip Registry

Since v0.8.0, Lip supports looking up teeth from a registry and installing them. You can submit your tooth to the registry to make it easier for others to install your tooth.

In this tutorial, we will submit a tooth to the official Lip registry (<https://registry.litebds.com>).

## Create a Lip Tooth

Follow the [Create a Lip Tooth](tutorials/create_a_lip_tooth.md) tutorial to create a Lip tooth. Make sure that you have a `tooth.json` file in the root directory of your tooth. And the **tooth** field is right the repository path of your tooth.

## Submit Your Tooth to Lip Registry

To submit your tooth to the registry, you need to create a pull request to the registry repository (<https://github.com/LiteLDev/Registry>).

You should create a new file in the `teeth` directory. The file name should be the alias of your tooth. The file content should be a simplified version of your `tooth.json`. The file content should be in JSON format and should contain the following fields. In this example, the alias of the tooth is `exampletool`. Therefore, the file name is `exampletool.json`.

```json
{
    "format_version": 1,
    "tooth": "example.com/exampleuser/exampletool",
    "information": {
        "name": "Example Tool",
        "description": "An example tool",
        "author": "Example User",
        "license": "MIT",
        "homepage": "example.com"
    }
}
```

Unlike the `tooth.json` under your tooth repository, every field in the registry file is required. The `format_version` field should be `1`.

After you create the file, you can create a pull request to the registry repository. The registry maintainers will review your pull request and merge it if it is valid.

## Install Your Tooth from Lip Registry

After your tooth is submitted to the registry, everyone can install it from the registry. You can use the `lip install` command to install your tooth from the registry.

```bash
lip install exampletool
```

## Update Your Tooth in Lip Registry

If you want to update your tooth in the registry, you need to update the registry file. You can update the registry file in the same way as submitting your tooth to the registry. The registry maintainers will review your pull request and merge it if it is valid.

## Remove Your Tooth from Lip Registry

If you want to remove your tooth from the registry, you need to remove the registry file. You can remove the registry file in the same way as submitting your tooth to the registry. The registry maintainers will review your pull request and merge it if it is valid.