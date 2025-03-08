# 安装

目前尚未实现 lip 安装器。您可以按照下面的说明手动安装 lip。

## lip CLI

为了简化安装过程，您可以使用以下命令：

对于 Windows (x64) 平台，请运行以下命令：
  
```shell
mkdir -p %LocalAppData%\lip
curl -L https://github.com/futrime/lip/releases/latest/download/lip-win-x64.zip -o %LocalAppData%\lip\lip.zip
tar -xf %LocalAppData%\lip\lip.zip -C %LocalAppData%\lip
del %LocalAppData%\lip\lip.zip
setx PATH "%PATH%;%LocalAppData%\lip"
```

对于 Windows (arm64) 平台，请将上面命令中的 `lip-win-x64.zip` 替换为 `lip-win-arm64.zip`。

对于 Linux，运行以下命令，并按照脚本中的提示完成安装：
  
```shell
curl -fsSL https://raw.githubusercontent.com/futrime/lip/HEAD/scripts/install_linux.sh | sh
```

对于 macOS，运行以下命令，并按照脚本中的提示完成安装：
  
```shell
curl -fsSL https://raw.githubusercontent.com/futrime/lip/HEAD/scripts/install_macos.sh | sh
```

若需手动安装 lip CLI，请从 [GitHub](https://github.com/futrime/lip/releases/latest) 下载适用于您平台的最新发行版压缩包，解压后将解压目录添加到您的 PATH 中。

## lip GUI

lip GUI 正在开发中，目前还无法安装。
