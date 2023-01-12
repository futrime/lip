package cmd

import (
	"flag"
	"fmt"
	"os"
	"path/filepath"

	"github.com/liteldev/lip/context"
)

const helpMessage = `
Usage:
  lip <command> [options]

Commands:
  cache                       Inspect and manage Lip's cache. (TO-DO)
  config                      Manage local and global configuration. (TO-DO)
  install                     Install teeth. (TO-DO)
  list                        List installed teeth. (TO-DO)
  show                        Show information about installed teeth. (TO-DO)
  tooth                       Maintain a tooth. (TO-DO)
  uninstall                   Uninstall teeth. (TO-DO)

General Options:
  -h, --help                  Show help.
  -V, --version               Show version and exit.`

const versionMessage = "Lip %s from %s"

func CmdLip() {
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
