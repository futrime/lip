# Tutorial: Create a Lip Tooth

This is the first part of a tutorial that introduces a few fundamental features of Lip. If you're just getting started with Lip, be sure to take a look at [Getting Started](getting_started.md) and [Creator's Guide](creator_s_guide.md), which introduces the basic commands of Lip.

In this tutorial you'll create a tooth containing a plugin of LiteLoaderBDS.

## Prerequisites

- **Some project management experience.** You ought to learn the basic usage of Git, and the basic syntax of JSON in advance.

- **A tool to edit tooth.json** Any text editor you have will work fine. The most popular are VSCode and Vim.

- **A command terminal** Lip works well with both PowerShell and cmd in Windows.

- **Lip command-line tool** You should install Lip in advance. For more information, refer to [Installation](installation.md)

## Prepare plugin distributions

Lip fetches all content of a version of a Git repository for installing. Therefore, you should get all files to be installed ready under the management of Git.

If you just work with text (e.g. script plugins, addons), you could just use the repository for development to create a tooth.

However, if you are working with binaries (e.g. native plugins, worlds), you might have to create another repository to store the content. Otherwise, the binaries may make your repository too large to manage.

In this example, we assume the repository structure as listed below:

```
exampleplugin.dll
exampleplugin/
  config.json
  libexample.dll
  data/
```

## Initialize the tooth

1. Open a command prompt and cd to the repository root. If you are using Windows, you can just press *shift* and right click in the file explorer, then click "Open PowerShell window here".

2. Run the command below to initialize the tooth. The command will create a tooth.json under the root of the repository.

   ```shell
   lip tooth init
   ```

3. Edit tooth.json. Fill in the content enclosed in pointed brackets ("<" and ">"). 

   ```json
   {
       "format_version": 1,
       "tooth": "example.com/exampleuser/exampleplugin",
       "version": "1.0.0",
       "dependencies": {
           "github.com/liteloaderbds-hub/liteloaderbds": [
               [
                   "2.9.x"
               ]
           ]
       },
       "information": {
           "name": "Example Plugin",
           "description": "An example plugin",
           "author": "Example User",
           "license": "MIT",
           "homepage": "example.com"
       },
       "placement": [
           {
               "source": "exampleplugin.dll",
               "destination": "plugins/exampleplugin.dll"
           },
           {
               "source": "exampleplugin/*",
               "destination": "plugins/exampleplugin/*"
           }
       ],
       "possession": [
           "plugins/exampleplugin/data/"
       ]
   }
   ```

   The tooth field indicates the tooth path of the tooth. If you would like to publish the tooth, it must be the tooth repository URL without protocol prefix (e.g. https:// or http://) in lowercase.

   The placement filed indicates how will Lip copy files from the tooth to the BDS. The source path bases on the root of the tooth (or the repository in this example and most cases), while the destination path bases on the root of BDS, in which "bedrock_server.exe" locates.
   
   The possession field indicates the private directory of this tooth. It will be removed when uninstalling the tooth but will be kept when reinstalling or upgrading the tooth. Note that the path indicated in the possession field bases on the root of BDS. And every item should ends with "/".

## Test the tooth

Before publishing the tooth, you should test it to make sure it works as expected.

1. Zip all files in the repository root, and rename the zip file to "exampleplugin.tth".

2. Copy the zip file to a certain directory, and then run the command below to install the tooth.

   ```shell
   lip install exampleplugin.tth
   ```

3. Run the command below to uninstall the tooth.

   ```shell
   lip uninstall exampleplugin.tth
   ```

4. Run the command below to install the tooth again.

   ```shell
   lip install exampleplugin.tth
   ```

5. Check if the tooth works as expected.

## Publish your tooth

1. Stash and commit the changes, and then push them to the public Git service.

2. Add a tag and publish a release with the version name. The tag name should be the version name added with prefix "v", e.g. "v1.0.0".

## Another example: make a Minecraft world a tooth

Generally, a Minecraft world has the following file structure:

```
Bedrock level/
  level.dat
  level.dat_old
  levelname.txt
  db/
```

You can create a tooth.json like this:

```json
{
    "format_version": 1,
    "tooth": "example.com/exampleuser/exampleworld",
    "version": "1.0.0",
    "dependencies": {},
    "information": {
        "name": "Example World",
        "description": "An example world",
        "author": "Example User",
        "license": "MIT",
        "homepage": "example.com"
    },
    "placement": [
        {
            "source": "Bedrock level/*",
            "destination": "worlds/exampleworld/*"
        }
    ],
    "possession": [
        "worlds/exampleworld/"
    ]
}
```

## Next Steps

You can read [tooth.json File Reference](../tooth_json_file_reference.md) for further reference.