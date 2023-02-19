# Tutorial: Submit Your Tooth to Lip Registry

Since v0.8.0, Lip supports looking up tooths from a registry and installing them. You can submit your tooth to the registry to make it easier for others to install your tooth.

In this tutorial, we will submit a tooth to the official Lip registry (<https://registry.litebds.com>).

## Create a Lip Tooth

Follow the [Create a Lip Tooth](tutorials/create_a_lip_tooth.md) tutorial to create a Lip tooth. Make sure that you have a `tooth.json` file in the root directory of your tooth. And the **tooth** field is right the repository path of your tooth.

## Submit Your Tooth to Lip Registry

To submit your tooth to the registry, you need to create a pull request to the registry repository (<https://github.com/LiteLDev/Registry>).

You should create a new file in the `tooths` directory. The file name should be the alias of your tooth. The file content should be a simplified version of your `tooth.json`. The file content should be in JSON format and should contain the following fields. In this example, the alias of the tooth is `lip`. Therefore, the file name is `lip.json`.

```json
{
    "format_version": 1,
    "tooth": "github.com/Tooth-Hub/Lip",
    "information": {
        "author": "LiteLDev",
        "description": "A package installer not only for LiteLoaderBDS",
        "homepage": "https://www.example.com",
        "license": "MIT",
        "name": "Lip",
        "repository": "github.com/LiteLDev/Lip",
        "tags": ["utility", "package-manager"]
    }
}
```

The `format_version`, `tooth`, `author`, `description` and `name` fields are required. The fields should also follow these rules:

- The `format_version` field should be `1`.
- The `tooth` field should be the tooth repository path without protocol prefix. Currently, we only accept tooths that are hosted on GitHub.
- The `author` field should be the GitHub username of the author of the tooth.
- The `description` field should be a one-line description of the tooth.
- The `homepage` field should be a valid URL with `http://` or `https://` prefix.
- The `license` field should be a valid [SPDX license identifier](https://spdx.org/licenses/) (including deprecated ones). For private tooth, just left it blank.
- The `repository` filed should be the project source code repository path without protocol prefix. Currently, we only accept repositories that are hosted on GitHub.
- The `tags` field should be an array of strings. Each string should be a valid tag. The tag can only contain lowercase letters, numbers and hyphens [a-z0-9-]. The tag should not start or end with a hyphen. The tag should not contain consecutive hyphens.

You may want to display a README page on the registry website. You can create a Markdown file with the same name as the registry file in `readmes` directory. For example, you can create a `lip.md` file. The content of the file will be displayed on the registry website.

After you create the file, you can create a pull request to the registry repository. The registry maintainers will review your pull request and merge it if it is valid.

### How to Set Tags

You can set tags for your tooth. The tags will be displayed on the registry website. You can set tags in the `tags` field of the registry file. The `tags` field should be an array of strings. Each string should be a valid tag. The tag can only contain lowercase letters, numbers and hyphens [a-z0-9-]. The tag should not start or end with a hyphen. The tag should not contain consecutive hyphens.

Some tags can be recognized by the registry website, Lip and LipUI. The recognized tags are:

Reserved tags:

- `featured`: The tooth is featured on the registry website. You should not set this tag manually. The registry maintainers will set this tag for you.

Type tags:

- `utility`: The tooth is a utility tool.
- `plugin`: The tooth is a plugin.
- `module`: The tooth is a module. This 
- `mod`: The tooth is a mod.
- `modpack`: The tooth is a modpack. This tag means that the tooth is a collection of mods.
- `addon`: The tooth is an addon.
- `world`: The tooth is a world.
- `integration`: The tooth is an integration pack. This tag means that the tooth is a collection of mods and plugins as well as the server software.

Ecosystem tags:

- `ll`: The tooth is for LiteLoaderBDS.
- `llse`: The tooth is for LiteLoaderBDS and depends on LiteLoaderBDS Script Engine.
- `llnet`: The tooth is for LiteLoaderBDS and depends on LiteLoader.NET.
- `bdsx`: The tooth is for BDSX.
- `pnx`: The tooth is for PowerNukkitX.
- `bds`: The tooth is for pure BDS.

## Install Your Tooth from Lip Registry

After your tooth is submitted to the registry, everyone can install it from the registry. You can use the `lip install` command to install your tooth from the registry.

```bash
lip install exampletool
```

## Update Your Tooth in Lip Registry

If you want to update your tooth in the registry, you need to update the registry file. You can update the registry file in the same way as submitting your tooth to the registry. The registry maintainers will review your pull request and merge it if it is valid.

## Remove Your Tooth from Lip Registry

If you want to remove your tooth from the registry, you need to remove the registry file. You can remove the registry file in the same way as submitting your tooth to the registry. The registry maintainers will review your pull request and merge it if it is valid.