# Tutorial: Create a lip Tooth

This is the first part of a tutorial that introduces a few fundamental features of lip. If you're just getting started with lip, be sure to take a look at [Getting Started](../quickstart.md), which introduces the basic commands of lip.

In this tutorial you'll create a tooth containing a plugin of LiteLoaderBDS.

## Prerequisites

- **Some project management experience.** You ought to learn the basic usage of Git, and the basic syntax of JSON in advance.

- **A tool to edit tooth.json** Any text editor you have will work fine. The most popular are VSCode and Vim.

- **A command terminal** lip works well with both PowerShell and cmd in Windows.

- **lip command-line tool** You should install lip in advance. For more information, refer to [Installation](../installation.md)

## Prepare plugin distributions

lip fetches all content of a version of a Git repository for installing. Therefore, you should get all files to be installed ready under the management of Git.

If you just work with text (e.g. script plugins, addons), you could just use the repository for development to create a tooth.

However, if you are working with binaries (e.g. native plugins, worlds), you might have to create another repository to store the content. Otherwise, the binaries may make your repository too large to manage.

In this example, we assume the repository structure as listed below:

```text
exampleplugin.dll
exampleplugin/
  config.json
  libexample.dll
  data/
```

## Initialize the tooth

Open a command prompt and cd to the repository root. If you are using Windows, you can just press *shift* and right click in the file explorer, then click "Open PowerShell window here".

Run the command below to initialize the tooth. The command will create a tooth.json under the root of the repository.

```shell
lip tooth init
```

Edit tooth.json. Fill in the content enclosed in pointed brackets ("<" and ">").

- The tooth field indicates the tooth path of the tooth. If you would like to publish the tooth, it must be the tooth repository URL without protocol prefix (e.g. https:// or http://) in lowercase.

- The placement filed indicates how will lip copy files from the tooth to the BDS. The source path bases on the root of the tooth (or the repository in this example and most cases), while the destination path bases on the root of BDS, in which "bedrock_server.exe" locates.

- The possession field indicates the private directory of this tooth. It will be removed when uninstalling the tooth but will be kept when reinstalling or upgrading the tooth. Note that the path indicated in the possession field bases on the root of BDS. And every item should ends with "/".

## Test the tooth

Before publishing the tooth, you should test it to make sure it works as expected.

Zip all files in the repository root, and rename the zip file to "exampleplugin.tth".

Copy the zip file to a certain directory, and then run the command below to install the tooth.

```shell
lip install exampleplugin.tth
```

Run the command below to uninstall the tooth.

```shell
lip uninstall exampleplugin.tth
```

Run the command below to install the tooth again.

```shell
lip install exampleplugin.tth
```

Check if the tooth works as expected.

## Publish your tooth

- Stash and commit the changes, and then push them to the public Git service.

- Add a tag and publish a release with the version name. The tag name should be the version name added with prefix "v", e.g. "v1.0.0".

## Another example: make a Minecraft world a tooth

Generally, a Minecraft world has the following file structure:

```text
Bedrock level/
  level.dat
  level.dat_old
  levelname.txt
  db/
```

You can create a tooth.json like this:

```json
{
    "format_version": 2,
    "tooth": "example.com/exampleuser/exampleworld",
    "version": "1.0.0",
    "dependencies": {},
    "information": {
        "name": "Example World",
        "description": "An example world",
        "author": "Example User",
        "tags": [
            "ll", "llbds", "bds"
        ]
    },
    "files": {
        "place": [
            {
                "src": "Bedrock level/*",
                "dest": "worlds/Bedrock level/"
            }
        ]
    }
}
```

## Next Steps

You can read [tooth.json File Reference](../tooth_json_file_reference.md) for further reference.
