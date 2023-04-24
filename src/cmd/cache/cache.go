package cmdlipcache

import (
	"flag"
	"os"

	cmdlipcachepurge "github.com/lippkg/lip/cmd/cache/purge"
	"github.com/lippkg/lip/utils/logger"
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
  purge                       Clear the cache.

Options:
  -h, --help                  Show help.`

// Run is the entry point.
func Run(args []string) {
	flagSet := flag.NewFlagSet("cache", flag.ExitOnError)

	// Rewrite the default usage message.
	flagSet.Usage = func() {
		logger.Info(helpMessage)
	}

	var flagDict FlagDict
	flagSet.BoolVar(&flagDict.helpFlag, "help", false, "")
	flagSet.BoolVar(&flagDict.helpFlag, "h", false, "")
	flagSet.Parse(args)

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		logger.Info(helpMessage)
		return
	}

	// If there is a subcommand, run it and exit.
	if flagSet.NArg() >= 1 {
		switch flagSet.Arg(0) {
		case "purge":
			cmdlipcachepurge.Run(args[1:])
			return
		default:
			logger.Error("Unknown command.")
			os.Exit(1)
		}
	}

	// If there is no subcommand, print help message and exit.
	logger.Info(helpMessage)
}
