# 常问问题

## 我在哪里可以找到在线可用的tooth？

访问[lipWeb](https://www.lippkg.com).

## 它的下载速度太慢了! 我可以做什么呢？

Lip通过GOPROXY下载依赖。你可以通过运行 `lip config GoModuleProxyURL <url>` 来使用更快的代理。Lip还支持GitHub镜像，你可以通过运行 `lip config GitHubMirrorURL <url>` 来使用它。如果您正在设置 HTTP 代理，您只需设置 `HTTP_PROXY` 和 `HTTPS_PROXY` 环境变量。

## 当我试图安装一个tooth时，它总是显示错误！

可能是缓存被破坏了。尝试通过运行 `lip cache purge` 来清除缓存。

## 我怎样才能更新lip？

删除当前的lip可执行文件并安装最新版本。
