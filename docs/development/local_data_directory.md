# Local Data Directory

The local data directory is the .lpm directory in the root of BDS. It contains these directories and files:

- records/

  The information of tooths installed


## records/

every JSON file should be name with the Base64 encoded tooth path without version.

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

  If true, Lip will not automatically remove or upgrade this tooth.
  