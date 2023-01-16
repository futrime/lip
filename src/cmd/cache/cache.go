package cmdlipcache

import (
	"flag"
	"os"

	cmdcachepurge "github.com/liteldev/lip/cmd/cache/purge"
	logger "github.com/liteldev/lip/utils/logger"
)

// FlagDict is a dictionary of flags.
type FlagDict struct {
	helpFlag bool
}

const helpMessage = `
Usage:
  lip cache [options]
  lip cache <command> [subcommand options] ...

Commands:
  purge                       Initialize and writes a new tooth.json file in the current directory.

Options:
  -h, --help                  Show help.`

// Run is the entry point.
func Run() {
	// If there is a subcommand, run it and exit.
	if len(os.Args) >= 3 {
		switch os.Args[2] {
		case "purge":
			cmdcachepurge.Run()
			return
		}
	}

	flagSet := flag.NewFlagSet("cache", flag.ExitOnError)

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

	// If there is no subcommand, print help message and exit.
	logger.Info(helpMessage)
}
