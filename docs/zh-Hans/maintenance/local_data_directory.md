# 本地数据目录

本地数据目录是位于BDS根部的.lpm目录。它包含这些目录和文件。

- records/

  每一个齿包的安装信息


## records/

每个JSON文件的名称都应该是带有Base64编码的齿包的路径，不含版本。

xxx.json

```json
{
    "format_version": 1,
    "tooth": "github.com/liteldev/liteloaderbds",
    "version": "2.9.0",
    "dependencies": {
        "libopenssl": ["1.1.0", "1.1.1"],
        "libopenssl3": [
            [">=3.0.5", "<=3.0.7"],
            "3.0.9"
        ],
        "libsqlite3": ["3.0.x"],
        "preloader": ["2.9.0"]
    },
    "information": {
        "name": "LiteLoaderBDS",
        "description": "Epoch-making and cross-language Bedrock Dedicated Server plugin loader.",
        "author": "LiteLDev",
        "license": "Modified LGPL-3.0",
        "homepage": "www.litebds.com"
    },
    "placement": [
        {
            "source": "",
            "destination": ""
        }
    ],
    "is_manually_installed": true
}
```

- is_manually_installed

  如果为真，Lip将不会自动删除或升级这个齿包。