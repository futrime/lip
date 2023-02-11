package cmdlipcache

import (
	"flag"

	cmdlipcachepurge "github.com/liteldev/lip/cmd/cache/purge"
	"github.com/liteldev/lip/utils/logger"
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
	// If there is a subcommand, run it and exit.
	if len(args) >= 1 {
		switch args[0] {
		case "purge":
			cmdlipcachepurge.Run(args[1:])
			return
		}
	}

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

	// If there is no subcommand, print help message and exit.
	logger.Info(helpMessage)
}
