package cmdliptooth

import (
	"flag"
	"os"

	cmdtoothinit "github.com/liteldev/lip/cmd/tooth/init"
	logger "github.com/liteldev/lip/utils/logger"
)

// FlagDict is a dictionary of flags.
type FlagDict struct {
	helpFlag bool
}

const helpMessage = `
Usage:
  lip tooth [options]
  lip tooth <command> [subcommand options] ...

Commands:
  init                        Initialize and writes a new tooth.json file in the current directory.

Options:
  -h, --help                  Show help.`

// Run is the entry point of the install command.
func Run() {
	// If there is a subcommand, run it and exit.
	if len(os.Args) >= 3 {
		switch os.Args[2] {
		case "init":
			cmdtoothinit.Run()
			return
		}
	}

	flagSet := flag.NewFlagSet("tooth", flag.ExitOnError)

	// Rewrite the default usage message.
	flagSet.Usage = func() {
		logger.Info(helpMessage)
	}

	var flagDict FlagDict

	flag.BoolVar(&flagDict.helpFlag, "help", false, "")
	flag.BoolVar(&flagDict.helpFlag, "h", false, "")

	flagSet.Parse(os.Args[2:])

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		logger.Info(helpMessage)
		return
	}

	// Default to help message.
	logger.Info(helpMessage)
}
