# LipUI Quickstart

To simplify the usage in the maximum extent, LipUI is provided. It is a GUI application that can be used to install, uninstall, and manage Lip tooths. It is also a good way to get started with Lip.

## Prerequisites

Before you start, you need to install Lip. You can find the installation guide [here](installation.md).

LipUI only runs on Windows. If you are using Linux or macOS, you can use the command-line interface of Lip.

LipUI depends on .NET 7.0 or .NET Framework 4.6.2. For most distributions of Windows 10, Windows 11, Windows Server 2019 and Windows Server 2022, .NET Framework 4.6 is bundled. Therefore, you are likely to be able to run LipUI directly. If you don't have .NET Framework installed, you can download .NET 7.0 [here](https://dotnet.microsoft.com/download/dotnet/7.0).

## Installation

LipUI is a portable application. You can download the latest version of LipUI [here](https://github.com/LipPkg/LipUI/releases/latest). You can put it anywhere you want.

## Usage

Just run `LipUI.exe` and you will see the main window of LipUI. First, you need to select a workspace. A workspace is a directory that contains all the tooths you installed. For Bedrock Server users, the workspace is the directory that contains `bedrock_server.exe`. You can add multiple workspaces. LipUI will automatically detect the tooths in the workspace.

![LipUI Main Window](../assets/img/lipui_main_window.png)

After you select a workspace, you can install, uninstall, and manage tooths. You can also use the search box to search for tooths.

![LipUI Registry](../assets/img/lipui_registry.png)

For tooths not in the registry, you can install them by clicking the `Install` button. You can also install tooths from a URL or a tooth file.

![LipUI Install Tooth](../assets/img/lipui_install_tooth.png)
