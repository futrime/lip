# lip uninstall

## 用法

```shell
lip uninstall [options] <tooth paths>

aliases: remove, rm, un, r
```

## 功能

卸载tooth。
本命令将会移除tooth所释放的文件，以及tooth作者指定该tooth占有的文件夹内容。

## 选择

- `-h, --help`

  展示帮助.

- `-y, --yes`

  跳过确认提示。

- `--keep-possession`

  保留tooth作者指定的tooth所占用的文件。这些文件通常是配置文件、数据文件等。