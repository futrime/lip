package cmd

func CmdInstall() {
	const helpMessage = `
Usage:
  lip install [options] <requirement specifier>
  lip install [options] <tooth url/path>

Description:
  Install a tooth from:

  - A tooth repository.
  - A local or remote standalone tooth file (with suffix .tt).

Options:
  -h, --help                  Show help.
  --dry-run                   Don't actually install anything, just print what would be.
  --upgrade                   Upgrade the specified tooth to the newest available version.
  --force-reinstall           Reinstall the tooth even if they are already up-to-date.`
}
