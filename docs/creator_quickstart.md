# Creator Quickstart

We're pleased that you are interested in creating with Lip.

## Ensure you have a working Lip

As a first step, you should check that you have a working Lip installed. This can be done by running the following commands and making sure that the output looks similar.

```shell
> lip --version
Lip 0.1.0 from C:\Users\ExampleUser\AppData\Local\Lip\lip.exe
```

## Common tasks

### Initialize a tooth workspace

```shell
> lip tooth init
What is the tooth path? (e.g. github.com/tooth-hub/llbds3)
github.com/tooth-hub/example
What is the name?
Example
What is the description?
An example tooth.
What is the author? Please input your GitHub username.
Bob
Successfully initialized a new tooth.
```

Then you can fill in the information in tooth.json to make your work recognized by Lip.

### Pack the tooth

Currently we have not provided commands to pack a tooth. You can just zip everything (make sure tooth.json is under the root of the zip file) and change its extension name from ".zip" to ".tth".

## GOPROXY related notice

Since we are using GOPROXY as the proxy to fetch tooth files, please DO NOT place a go.mod file under the root of your repository.

## Next Steps

You can read [tooth.json File Reference](tooth_json_file_reference.md) for more information.
