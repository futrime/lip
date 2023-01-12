package cmd

import (
	"flag"
	"fmt"
	"os"
	"path/filepath"

	"github.com/liteldev/lip/context"
)

func CmdLip() {
	const helpMessage = `
Usage:
  lip [options]
  lip <command> [subcommand options]

Commands:
  cache                       Inspect and manage Lip's cache. (TO-DO)
  config                      Manage local and global configuration. (TO-DO)
  install                     Install a tooth. (TO-DO)
  list                        List installed teeth. (TO-DO)
  show                        Show information about installed teeth. (TO-DO)
  tooth                       Maintain a tooth. (TO-DO)
  uninstall                   Uninstall a tooth. (TO-DO)

Options:
  -h, --help                  Show help.
  -V, --version               Show version and exit.`

	const versionMessage = "Lip %s from %s"

	flag.Usage = func() {
		fmt.Println(helpMessage)
	}

	var helpFlag bool
	flag.BoolVar(&helpFlag, "help", false, "")
	flag.BoolVar(&helpFlag, "h", false, "")

	var versionFlag bool
	flag.BoolVar(&versionFlag, "version", false, "")
	flag.BoolVar(&versionFlag, "V", false, "")

	flag.Parse()

	if helpFlag {
		fmt.Println(helpMessage)
	} else if versionFlag {
		exPath, _ := filepath.Abs(os.Args[0])
		fmt.Printf(versionMessage, context.Version, exPath)
	} else {
		fmt.Println(helpMessage)
	}
}
