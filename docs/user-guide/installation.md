# Installation

So far, lip installer has not been implemented. You can install lip manually by following the instructions below.

## lip CLI

To ease the process, you can use the following commands:

For Windows (x64), run the following commands:
  
```shell
mkdir -p %LocalAppData%\lip
curl -L https://github.com/futrime/lip/releases/latest/download/lip-win-x64.zip -o %LocalAppData%\lip\lip.zip
tar -xf %LocalAppData%\lip\lip.zip -C %LocalAppData%\lip
del %LocalAppData%\lip\lip.zip
setx PATH "%PATH%;%LocalAppData%\lip"
```

For Windows (arm64), simply replace `lip-win-x64.zip` with `lip-win-arm64.zip` in the above commands.

For Linux, run the following command and follow the instructions in the script to complete the installation:
  
```shell
curl -fsSL https://raw.githubusercontent.com/futrime/lip/HEAD/scripts/install_linux.sh | sh
```

For macOS, run the following command and follow the instructions in the script to complete the installation:
  
```shell
curl -fsSL https://raw.githubusercontent.com/futrime/lip/HEAD/scripts/install_macos.sh | sh
```

To install lip CLI manually, download the latest release archive for your platform from [GitHub](https://github.com/futrime/lip/releases/latest), extract it, and add the extracted directory to your PATH.

## lip GUI

lip GUI is under development and not yet available for installation.
