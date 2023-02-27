# lip exec

## 用法

```shell
lip exec [options] <tool> [args...]
```

## 功能

执行一个Lip工具。工具应该先用`lip install`来安装。


事实上，这将执行 ./lip/tools/\<tool>/\<tool> (在Windows上将会执行 .\lip\tools\\\<tool>\\\<tool>.exe 或 .\lip\tools\\\<tool>\\\<tool>.cmd ).

## 选项

- `-h, --help`

  展示帮助。

## 样例

你甚至可以自己执行Lip。

```shell
lip exec lip list
```