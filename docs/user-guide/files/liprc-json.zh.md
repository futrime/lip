# liprc.json

`liprc.json`文件作为lip的配置文件，使您能够配置缓存、代理和脚本执行等设置。lip将此文件存储在Windows的`%APPDATA%\lip\liprc.json`、Linux的`~/.config/lip/liprc.json`和macOS的`~/Library/Application Support/lip/liprc.json`中。

项目特定的设置优先于用户范围的设置。所有配置字段都是可选的。

## 配置字段

### cache

- 类型: `string`
- 默认值: Windows: `%LocalAppData%\lip\cache`, Linux: `~/.local/share/lip/cache`, macOS: `~/Library/Application Support/lip/cache`

定义存储缓存文件的目录路径。

### github_proxies

- 类型: `string`
- 默认值: "" (空字符串)

设置GitHub连接的代理URL（用逗号分隔）。当为空时，lip会尝试直连GitHub。

### go_module_proxies

- 类型: `string`
- 默认值: `https://goproxy.io`

定义Go模块下载的Go模块代理URL（用逗号分隔）。
