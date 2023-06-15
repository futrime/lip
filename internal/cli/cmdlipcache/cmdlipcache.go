package cmdlipcache

import (
	"flag"
	"fmt"

	"github.com/lippkg/lip/internal/cli/cmdlipcachepurge"
	"github.com/lippkg/lip/internal/contexts"
	"github.com/lippkg/lip/internal/logging"
)

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
  -h, --help                  Show help.
`

func Run(ctx contexts.Context, args []string) error {
	var err error

	flagSet := flag.NewFlagSet("cache", flag.ExitOnError)

	// Rewrite the default usage message.
	flagSet.Usage = func() {
		// Do nothing.
	}

	var flagDict FlagDict
	flagSet.BoolVar(&flagDict.helpFlag, "help", false, "")
	flagSet.BoolVar(&flagDict.helpFlag, "h", false, "")
	err = flagSet.Parse(args[1:])
	if err != nil {
		return fmt.Errorf("failed to parse flags: %w", err)
	}

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		logging.Info(helpMessage)
		return nil
	}

	// If there is a subcommand, run it and exit.
	if flagSet.NArg() >= 1 {
		switch flagSet.Arg(0) {
		case "purge":
			err = cmdlipcachepurge.Run(ctx, args[1:])
			if err != nil {
				return fmt.Errorf("failed to run the 'purge' command: %w", err)
			}
			return nil

		default:
			return fmt.Errorf("unknown command: lip cache %v", flagSet.Arg(0))
		}
	}

	return fmt.Errorf(
		"no command specified. See 'lip cache --help' for more information")
}
