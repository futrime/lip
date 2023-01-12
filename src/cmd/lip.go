package cmd

import (
	"flag"
	"fmt"
	"os"
	"path/filepath"

	context "github.com/liteldev/lip/context"
	"github.com/liteldev/lip/utils/logger"
)

type FlagDict struct {
	helpFlag    bool
	versionFlag bool
}

func Run() {
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

	// Rewrite the default usage message.
	flag.Usage = func() {
		logger.Info(helpMessage)
	}

	var flagDict FlagDict

	flag.BoolVar(&flagDict.helpFlag, "help", false, "")
	flag.BoolVar(&flagDict.helpFlag, "h", false, "")

	flag.BoolVar(&flagDict.versionFlag, "version", false, "")
	flag.BoolVar(&flagDict.versionFlag, "V", false, "")

	flag.Parse()

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		logger.Info(helpMessage)
		return
	}

	if flagDict.versionFlag {
		exPath, _ := filepath.Abs(os.Args[0])
		logger.Info(versionMessage, context.Version, exPath)
		return
	}

	// Default to help message.
	logger.Error("No command specified.")
	fmt.Println(helpMessage)
}
