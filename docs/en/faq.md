# Frequent Asked Questions

## Where can I find online available tooths?

You can find them on [registry.litebds.com](https://registry.litebds.com).

## Can I use registries other than `registry.litebds.com` ?

Of course! You can use any registries you want. Just sets the `LIP_REGISTRY` environment variable to the registry you want to use, e.g. `LIP_REGISTRY=https://registry.litebds.com`.

## It downloads so slowly! What can I do?

Lip downloads tooths via GOPROXY. You can set the `LIP_GOPROXY` environment variable to a list of GOPROXY servers seperated by commas, e.g. `LIP_GOPROXY=https://goproxy.cn,https://goproxy.io`. Set a GOPROXY server that is close to you.

## It always shows errors when I try to install a tooth!

Probably the cache is corrupted. Try to purge the cache by running `lip cache purge`.

## How can I update Lip?

For global installation, you have to download the latest version of Lip and replace the old one. For local installation, you can run `lip install --upgrade lip` to update Lip.