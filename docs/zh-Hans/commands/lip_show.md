# lip show

## Usage

```shell
lip show [options] <tooth path>
```

## 功能

展示tooth的信息 如果没有安装，只显示版本列表。

输出是符合RFC标准的邮件头格式。

## 选项

- `-h, --help`

  展示帮助。

- `--files`

  显示已安装文件的完整列表。

- `--available`

  显示可用版本的完整列表。

- `--json`
  
  以JSON格式输出。(不能用`--quiet`隐藏)